using Nickvision.Desktop.Application;
using Nickvision.Desktop.Filesystem;
using Nickvision.Desktop.Tests.Mocks;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nickvision.Desktop.Tests;

[TestClass]
public class UpdaterDownloadTests
{
    [TestMethod]
    public async Task ReplacesFileOnlyAfterVerification()
    {
        using var download = new DownloadFixture();
        await File.WriteAllTextAsync(download.Path, "installed version");
        download.BeforeDigest = () => Assert.AreEqual("installed version", File.ReadAllText(download.Path));

        Assert.IsTrue(await download.RunAsync());
        CollectionAssert.AreEqual(download.Payload, await File.ReadAllBytesAsync(download.Path));
        download.AssertNoTemporaryFiles();
    }

    [TestMethod]
    public async Task CreatesNewFile()
    {
        using var download = new DownloadFixture();

        Assert.IsTrue(await download.RunAsync());
        CollectionAssert.AreEqual(download.Payload, await File.ReadAllBytesAsync(download.Path));
        download.AssertNoTemporaryFiles();
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task ChecksumFailureDoesNotInstallFile(bool existingFile)
    {
        using var download = new DownloadFixture();
        if (existingFile)
        {
            await File.WriteAllTextAsync(download.Path, "installed version");
        }
        download.Digest = new string('0', 64);

        Assert.IsFalse(await download.RunAsync());
        if (existingFile)
        {
            Assert.AreEqual("installed version", await File.ReadAllTextAsync(download.Path));
        }
        else
        {
            Assert.IsFalse(File.Exists(download.Path));
        }
        download.AssertNoTemporaryFiles();
    }

    [TestMethod]
    public async Task InterruptedDownloadPreservesFile()
    {
        using var download = new DownloadFixture();
        await File.WriteAllTextAsync(download.Path, "installed version");
        download.InterruptDownload = true;

        Assert.IsFalse(await download.RunAsync());
        Assert.AreEqual("installed version", await File.ReadAllTextAsync(download.Path));
        download.AssertNoTemporaryFiles();
    }

    [TestMethod]
    public async Task DigestRequestFailurePreservesFile()
    {
        using var download = new DownloadFixture();
        await File.WriteAllTextAsync(download.Path, "installed version");
        download.DigestStatusCode = HttpStatusCode.ServiceUnavailable;

        Assert.IsFalse(await download.RunAsync());
        Assert.AreEqual("installed version", await File.ReadAllTextAsync(download.Path));
        download.AssertNoTemporaryFiles();
    }

    [TestMethod]
    public async Task FailedReplacementRemovesTemporaryFile()
    {
        using var download = new DownloadFixture();
        Directory.CreateDirectory(download.Path);
        var marker = Path.Combine(download.Path, "keep");
        await File.WriteAllTextAsync(marker, "unchanged");

        Assert.IsFalse(await download.RunAsync());
        Assert.AreEqual("unchanged", await File.ReadAllTextAsync(marker));
        download.AssertNoTemporaryFiles();
    }

    [TestMethod]
    public async Task ReplacesRunningLinuxExecutableAndPreservesPermissions()
    {
        if (!OperatingSystem.IsLinux())
        {
            Assert.Inconclusive("Linux executable replacement regression test.");
            return;
        }
        using var download = new DownloadFixture(await File.ReadAllBytesAsync("/bin/echo"));
        File.Copy("/bin/sleep", download.Path);
        var mode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
        File.SetUnixFileMode(download.Path, mode);
        using var running = Process.Start(new ProcessStartInfo(download.Path, "60") { UseShellExecute = false });
        Assert.IsNotNull(running);
        try
        {
            // Establish the same ETXTBSY condition as an active yt-dlp process.
            Assert.Throws<IOException>(() =>
            {
                using var stream = new FileStream(download.Path, FileMode.Open, FileAccess.Write);
            });
            Assert.IsTrue(await download.RunAsync());
            Assert.IsFalse(running.HasExited);
            CollectionAssert.AreEqual(download.Payload, await File.ReadAllBytesAsync(download.Path));
            Assert.AreEqual(mode, File.GetUnixFileMode(download.Path));
            download.AssertNoTemporaryFiles();
            using var updated = Process.Start(new ProcessStartInfo(download.Path, "updated")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            Assert.IsNotNull(updated);
            Assert.AreEqual("updated", (await updated.StandardOutput.ReadToEndAsync()).Trim());
            await updated.WaitForExitAsync();
            Assert.AreEqual(0, updated.ExitCode);
        }
        finally
        {
            if (!running.HasExited)
            {
                running.Kill();
            }
            await running.WaitForExitAsync();
        }
    }

    private sealed class DownloadFixture : IDisposable
    {
        private readonly string _directory;
        private readonly string _cachePath;
        private readonly HttpClient _client;
        private readonly UpdaterService _updater;

        public string Path { get; }
        public byte[] Payload { get; }
        public string Digest { get; set; }
        public bool InterruptDownload { get; set; }
        public HttpStatusCode DigestStatusCode { get; set; } = HttpStatusCode.OK;
        public Action? BeforeDigest { get; set; }

        public DownloadFixture(byte[]? payload = null)
        {
            _directory = Directory.CreateTempSubdirectory("nickvision-updater-").FullName;
            Path = global::System.IO.Path.Combine(_directory, "yt-dlp");
            Payload = payload ?? Encoding.UTF8.GetBytes("updated version");
            Digest = Convert.ToHexStringLower(SHA256.HashData(Payload));
            var owner = $"updater-test-{Guid.NewGuid():N}";
            _cachePath = global::System.IO.Path.Combine(UserDirectories.Cache, $"{owner}-test-releases.json");
            _client = new HttpClient(new ResponseHandler(Respond));
            _updater = new UpdaterService(new MockLogger<UpdaterService>(), owner, "test", _client);
        }

        public Task<bool> RunAsync() => _updater.DownloadReleaseAssetAsync(new AppVersion("1.0.0"), Path, "yt-dlp");

        public void AssertNoTemporaryFiles()
        {
            foreach (var entry in Directory.GetFileSystemEntries(_directory))
            {
                Assert.AreEqual(Path, entry, "An unexpected temporary file remains.");
            }
        }

        public void Dispose()
        {
            _updater.Dispose();
            _client.Dispose();
            File.Delete(_cachePath);
            Directory.Delete(_directory, true);
        }

        private HttpResponseMessage Respond(HttpRequestMessage request)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/releases"))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""
                        [{"tag_name":"1.0.0","assets":[{"name":"yt-dlp","browser_download_url":"https://example.test/asset","url":"https://example.test/digest"}]}]
                        """, Encoding.UTF8, "application/json")
                };
            }
            if (request.RequestUri.AbsolutePath == "/asset")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = InterruptDownload ? new StreamContent(new InterruptedStream(Payload)) : new ByteArrayContent(Payload)
                };
            }
            if (request.RequestUri.AbsolutePath == "/digest")
            {
                BeforeDigest?.Invoke();
                return new HttpResponseMessage(DigestStatusCode)
                {
                    Content = new StringContent($"{{\"digest\":\"sha256:{Digest}\"}}", Encoding.UTF8, "application/json")
                };
            }
            throw new InvalidOperationException($"Unexpected request: {request.RequestUri}");
        }
    }

    private sealed class ResponseHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(respond(request));
    }

    private sealed class InterruptedStream(byte[] payload) : MemoryStream(payload)
    {
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (Position > 0)
            {
                throw new IOException("Simulated interrupted download.");
            }
            return base.ReadAsync(buffer, offset, Math.Min(count, 4), cancellationToken);
        }
    }
}
