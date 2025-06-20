﻿using System.Text.Json;
using Aria2NET.Apis;

namespace Aria2NET;

/// <summary>
///     Documentation about the API can be found here:
///     https://aria2.github.io/manual/en/html/aria2c.html#json-rpc-using-http-get
/// </summary>
public class Aria2NetClient
{
    private readonly Requests _requests;

    /// <summary>
    ///     Initialize the Aria2NetClient API.
    /// </summary>
    /// <param name="aria2Url">
    ///     The URL to your aria2 instance. Must end in /jsonrpc, for example: http://127.0.0.1:6801/jsonrpc.
    ///     To use SSL, use https.
    /// </param>
    /// <param name="secret">
    ///     Optional secret to your RPC instance.
    /// </param>
    /// <param name="httpClient">
    ///     Optional HttpClient if you want to use your own HttpClient.
    /// </param>
    /// <param name="retryCount">
    ///     Optional amount of tries a request should do before marking it as error.
    /// </param>
    public Aria2NetClient(String aria2Url, String? secret = null, HttpClient? httpClient = null, Int32 retryCount = 0)
    {
        if (!aria2Url.EndsWith("/jsonrpc"))
        {
            throw new Exception($"The URL must end with /jsonrpc");
        }

        var store = new Store
        {
            Aria2Url = aria2Url,
            Secret = secret,
            RetryCount = retryCount
        };

        var client = httpClient ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        _requests = new Requests(client, store);
    }

    /// <summary>
    ///     This method adds a new download.
    /// </summary>
    /// <param name="uriList">
    ///     UriList is a list of HTTP/FTP/SFTP/BitTorrent URIs (strings) pointing to the same resource. If you mix URIs
    ///     pointing to different resources, then the download may fail or be corrupted without aria2 complaining. When adding
    ///     BitTorrent Magnet URIs, uris must have only one element and it should be BitTorrent Magnet URI.
    /// </param>
    /// <param name="options">
    ///     For all available options see: https://aria2.github.io/manual/en/html/aria2c.html#id2.
    /// </param>
    /// <param name="position">
    ///     If position is given, it must be an integer starting from 0. The new download will be inserted at position in the
    ///     waiting queue. If position is omitted or position is larger than the current size of the queue, the new download is
    ///     appended to the end of the queue.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>The GID of the newly registered download</returns>
    public async Task<String> AddUriAsync(IList<String> uriList, IDictionary<String, Object>? options = null, Int32? position = null, CancellationToken cancellationToken = default)
    {
        if (uriList == null || uriList.Count == 0)
        {
            throw new ArgumentException("UriList cannot be null or empty");
        }

        options ??= new Dictionary<String, Object>();

        return await _requests.GetRequestAsync<String>("aria2.addUri", cancellationToken, uriList, options, position);
    }

    /// <summary>
    ///     This method adds a new BitTorrent download.
    /// </summary>
    /// <param name="torrent">
    ///     Contents of the .torrent file.
    /// </param>
    /// <param name="uriList">
    ///     Uris is used for Web-seeding. For single file torrents, the URI can be a complete URI pointing to the resource; if
    ///     URI ends with /, name in torrent file is added. For multi-file torrents, name and path in torrent are added to form
    ///     a URI for each file.
    /// </param>
    /// <param name="options">
    ///     For all available options see: https://aria2.github.io/manual/en/html/aria2c.html#id2.
    /// </param>
    /// <param name="position">
    ///     If position is given, it must be an integer starting from 0. The new download will be inserted at position in the
    ///     waiting queue. If position is omitted or position is larger than the current size of the queue, the new download is
    ///     appended to the end of the queue.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>The GID of the newly registered download.</returns>
    public async Task<String> AddTorrentAsync(Byte[] torrent,
                                              IList<String>? uriList = null,
                                              IDictionary<String, Object>? options = null,
                                              Int32? position = null,
                                              CancellationToken cancellationToken = default)
    {
        var torrentFile = Convert.ToBase64String(torrent);

        uriList ??= new List<String>();
        options ??= new Dictionary<String, Object>();

        return await _requests.GetRequestAsync<String>("aria2.addTorrent", cancellationToken, torrentFile, uriList, options, position);
    }

    /// <summary>
    ///     This method returns the version of aria2 and the list of enabled features.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>The version and enabled features.</returns>
    public async Task<VersionResult> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<VersionResult>("aria2.getVersion", cancellationToken);
    }

    /// <summary>
    ///     This method adds a Metalink download.
    /// </summary>
    /// <param name="torrent">
    ///     Contents of the .metalink file.
    /// </param>
    /// <param name="options">
    ///     For all available options see: https://aria2.github.io/manual/en/html/aria2c.html#id2.
    /// </param>
    /// <param name="position">
    ///     If position is given, it must be an integer starting from 0. The new download will be inserted at position in the
    ///     waiting queue. If position is omitted or position is larger than the current size of the queue, the new download is
    ///     appended to the end of the queue.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of GIDs of newly registered downloads.</returns>
    public async Task<List<String>> AddMetalinkAsync(Byte[] torrent,
                                                     IDictionary<String, Object>? options = null,
                                                     Int32? position = null,
                                                     CancellationToken cancellationToken = default)
    {
        var torrentFile = Convert.ToBase64String(torrent);

        options ??= new Dictionary<String, Object>();

        return await _requests.GetRequestAsync<List<String>>("aria2.addMetalink", cancellationToken, torrentFile, options, position);
    }

    /// <summary>
    ///     This method removes the download denoted by gid. If the specified download is in progress, it is first
    ///     stopped. The status of the removed download becomes removed.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The GID of removed download.</returns>
    public async Task<String> RemoveAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<String>("aria2.remove", cancellationToken, gid);
    }

    /// <summary>
    ///     This method removes the download denoted by gid. This method behaves just like aria2.remove() except that this
    ///     method removes the download without performing any actions which take time, such as contacting BitTorrent trackers
    ///     to unregister the download first.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The GID of removed download.</returns>
    public async Task<String> ForceRemoveAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<String>("aria2.forceRemove", cancellationToken, gid);
    }

    /// <summary>
    ///     This method pauses the download denoted by gid (string). The status of paused download becomes paused. If the
    ///     download was active, the download is placed in the front of waiting queue. While the status is paused, the download
    ///     is not started. To change status to waiting, use the aria2.unpause() method.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The GID of paused download.</returns>
    public async Task<String> PauseAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<String>("aria2.pause", cancellationToken, gid);
    }

    /// <summary>
    ///     This method is equal to calling Pause() for every active/waiting download.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>True if successful.</returns>
    public async Task<Boolean> PauseAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _requests.GetRequestAsync<String>("aria2.pauseAll", cancellationToken);

        return result == "OK";
    }

    /// <summary>
    ///     This method pauses the download denoted by gid. This method behaves just like aria2.pause() except that this method
    ///     pauses downloads without performing any actions which take time, such as contacting BitTorrent trackers to
    ///     unregister the download first.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The GID of paused download.</returns>
    public async Task<String> ForcePauseAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<String>("aria2.forcePause", cancellationToken, gid);
    }

    /// <summary>
    ///     This method is equal to calling ForcePause() for every active/waiting download.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>True if successful.</returns>
    public async Task<Boolean> ForcePauseAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _requests.GetRequestAsync<String>("aria2.forcePauseAll", cancellationToken);

        return result == "OK";
    }

    /// <summary>
    ///     This method changes the status of the download denoted by gid (string) from paused to waiting, making the download
    ///     eligible to be restarted.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The GID of unpaused download.</returns>
    public async Task<String> UnpauseAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<String>("aria2.unpause", cancellationToken, gid);
    }

    /// <summary>
    ///     This method is equal to calling ForcePause() for every active/waiting download.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>True if successful.</returns>
    public async Task<Boolean> UnpauseAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _requests.GetRequestAsync<String>("aria2.unpauseAll", cancellationToken);

        return result == "OK";
    }

    /// <summary>
    ///     This method returns the progress of the download denoted by gid (string). keys is an array of strings.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Status of the download.</returns>
    public async Task<DownloadStatusResult> TellStatusAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<DownloadStatusResult>("aria2.tellStatus", cancellationToken, gid);
    }

    /// <summary>
    ///     This method returns the URIs used in the download denoted by gid (string).
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of urls.</returns>
    public async Task<IList<UriResult>> GetUrisAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<UriResult>>("aria2.getUris", cancellationToken, gid);
    }

    /// <summary>
    ///     This method returns the file list of the download denoted by gid (string).
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of files.</returns>
    public async Task<IList<FileResult>> GetFilesAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<FileResult>>("aria2.getFiles", cancellationToken, gid);
    }

    /// <summary>
    ///     This method returns a list peers of the download denoted by gid (string). This method is for BitTorrent only. The
    ///     response is an array of structs and contains the following keys. Values are strings.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of files.</returns>
    public async Task<IList<PeerResult>> GetPeersAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<PeerResult>>("aria2.getPeers", cancellationToken, gid);
    }

    /// <summary>
    ///     This method returns currently connected HTTP(S)/FTP/SFTP servers of the download denoted by gid (string). The
    ///     response is an array of structs and contains the following keys. Values are strings.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of files.</returns>
    public async Task<IList<ServerResult>> GetServersAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<ServerResult>>("aria2.getServers", cancellationToken, gid);
    }

    /// <summary>
    ///     This method changes the position of the download denoted by gid in the queue. pos is an integer. how is a string.
    ///     If how is POS_SET, it moves the download to a position relative to the beginning of the queue. If how is POS_CUR,
    ///     it moves the download to a position relative to the current position. If how is POS_END, it moves the download to a
    ///     position relative to the end of the queue. If the destination position is less than 0 or beyond the end of the
    ///     queue, it moves the download to the beginning or the end of the queue respectively. The response is an integer
    ///     denoting the resulting position.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="pos">The new position.</param>
    /// <param name="how">The method of setting the new position.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The new position of the download in the queue.</returns>
    public async Task<Int32> ChangePositionAsync(String gid, Int32 pos, ChangePositionHow how, CancellationToken cancellationToken = default)
    {
        var howString = how switch
        {
            ChangePositionHow.FromCurrent => "POS_SET",
            ChangePositionHow.FromEnd => "POS_CUR",
            _ => "POS_END"
        };

        return await _requests.GetRequestAsync<Int32>("aria2.changePosition", cancellationToken, gid, pos, howString);
    }

    /// <summary>
    ///     This method returns a list of active downloads. The response is an array of the same structs as returned by the
    ///     aria2.tellStatus() method.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<DownloadStatusResult>> TellActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<DownloadStatusResult>>("aria2.tellActive", cancellationToken);
    }

    /// <summary>
    ///     This method returns a list of waiting downloads, including paused ones. offset is an integer and specifies the
    ///     offset from the download waiting at the front. num is an integer and specifies the max. number of downloads to be
    ///     returned. For the keys parameter, please refer to the aria2.tellStatus() method.
    ///     If offset is a positive integer, this method returns downloads in the range of [offset, offset + num).
    ///     offset can be a negative integer. offset == -1 points last download in the waiting queue and offset == -2 points
    ///     the download before the last download, and so on. Downloads in the response are in reversed order then.
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="num"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<DownloadStatusResult>> TellWaitingAsync(Int32 offset, Int32 num, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<DownloadStatusResult>>("aria2.tellWaiting", cancellationToken, offset, num);
    }

    /// <summary>
    ///     This method returns a list of stopped downloads. offset is an integer and specifies the offset from the least
    ///     recently stopped download. num is an integer and specifies the max. number of downloads to be returned. For the
    ///     keys parameter, please refer to the aria2.tellStatus() method. offset and num have the same semantics as described
    ///     in the aria2.tellWaiting() method.
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="num"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<DownloadStatusResult>> TellStoppedAsync(Int32 offset, Int32 num, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<DownloadStatusResult>>("aria2.tellStopped", cancellationToken, offset, num);
    }

    /// <summary>
    ///     This method returns a list of the all active, the first 1000 stopped and first 1000 waiting downloads.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<DownloadStatusResult>> TellAllAsync(CancellationToken cancellationToken = default)
    {
        var results = await _requests.MultiRequestAsync(cancellationToken, 
                                                        new Object[] { "aria2.tellStopped", 0, 1000 },
                                                        new Object[] { "aria2.tellWaiting", 0, 1000 },
                                                        new Object[] { "aria2.tellActive" });

        var results1 = JsonSerializer.Deserialize<List<DownloadStatusResult>>(((JsonElement)results[0]).GetRawText()) ?? new List<DownloadStatusResult>();
        var results2 = JsonSerializer.Deserialize<List<DownloadStatusResult>>(((JsonElement)results[1]).GetRawText()) ?? new List<DownloadStatusResult>();
        var results3 = JsonSerializer.Deserialize<List<DownloadStatusResult>>(((JsonElement)results[2]).GetRawText()) ?? new List<DownloadStatusResult>();
            
        return results1.Concat(results2).Concat(results3).ToList();
    }
        
    /// <summary>
    ///     This method returns options of the download denoted by gid. The response is a struct where keys are the names of
    ///     options. The values are strings. Note that this method does not return options which have no default value and have
    ///     not been set on the command-line, in configuration files or RPC methods.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IDictionary<String, String>> GetOptionAsync(String gid, CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<Dictionary<String, String>>("aria2.getOption", cancellationToken, gid);
    }

    /// <summary>
    ///     This method removes the URIs in delUris from and appends the URIs in addUris to download denoted by gid. delUris
    ///     and addUris are lists of strings. A download can contain multiple files and URIs are attached to each file.
    ///     fileIndex is used to select which file to remove/attach given URIs. fileIndex is 1-based. position is used to
    ///     specify where URIs are inserted in the existing waiting URI list. position is 0-based. When position is omitted,
    ///     URIs are appended to the back of the list. This method first executes the removal and then the addition. position
    ///     is the position after URIs are removed, not the position when this method is called. When removing an URI, if the
    ///     same URIs exist in download, only one of them is removed for each URI in delUris. In other words, if there are
    ///     three URIs http://example.org/aria2 and you want remove them all, you have to specify (at least) 3
    ///     http://example.org/aria2 in delUris. This method returns a list which contains two integers. The first integer is
    ///     the number of URIs deleted. The second integer is the number of URIs added.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="fileIndex">The index of the download to change the URL's for.</param>
    /// <param name="delUris">A list of URI's to remove from the download.</param>
    /// <param name="addUris">A list of URI's to add to the download.</param>
    /// <param name="position">The position of the URLs.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<String>> ChangeUriAsync(String gid,
                                                    Int32 fileIndex,
                                                    IList<String> delUris,
                                                    IList<String> addUris,
                                                    Int32? position = null,
                                                    CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<String>>("aria2.changeUri", cancellationToken, gid, fileIndex, delUris, addUris, position);
    }

    /// <summary>
    ///     This method changes options of the download denoted by gid (string) dynamically. options is a struct. The options
    ///     listed in Input File subsection are available, except for following options:
    ///     dry-run
    ///     metalink-base-uri
    ///     parameterized-uri
    ///     pause
    ///     piece-length
    ///     rpc-save-upload-metadata
    ///     Except for the following options, changing the other options of active download makes it restart (restart itself is
    ///     managed by aria2, and no user intervention is required):
    ///     bt-max-peers
    ///     bt-request-peer-speed-limit
    ///     bt-remove-unselected-file
    ///     force-save
    ///     max-download-limit
    ///     max-upload-limit
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="options">
    ///     The options to change.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ChangeOptionAsync(String gid, IDictionary<String, String> options, CancellationToken cancellationToken = default)
    {
        await _requests.GetRequestAsync("aria2.changeOption", cancellationToken, gid, options);
    }

    /// <summary>
    ///     This method returns the global options. The response is a struct. Its keys are the names of options. Values are
    ///     strings. Note that this method does not return options which have no default value and have not been set on the
    ///     command-line, in configuration files or RPC methods. Because global options are used as a template for the options
    ///     of newly added downloads, the response contains keys returned by the aria2.getOption() method.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IDictionary<String, String>> GetGlobalOptionAsync(CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<Dictionary<String, String>>("aria2.getGlobalOption", cancellationToken);
    }

    /// <summary>
    ///     This method changes global options dynamically. options is a struct. The following options are available:
    ///     bt-max-open-files
    ///     download-result
    ///     keep-unfinished-download-result
    ///     log
    ///     log-level
    ///     max-concurrent-downloads
    ///     max-download-result
    ///     max-overall-download-limit
    ///     max-overall-upload-limit
    ///     optimize-concurrent-downloads
    ///     save-cookies
    ///     save-session
    ///     server-stat-of
    ///     In addition, options listed in the Input File subsection are available, except for following options: checksum,
    ///     index-out, out, pause and select-file.
    ///     With the log option, you can dynamically start logging or change log file. To stop logging, specify an empty
    ///     string("") as the parameter value. Note that log file is always opened in append mode.
    /// </summary>
    /// <param name="options">The options to change.</param>
    /// <param name="cancellationToken"></param>
    public async Task ChangeGlobalOptionAsync(IDictionary<String, String> options, CancellationToken cancellationToken = default)
    {
        await _requests.GetRequestAsync("aria2.changeGlobalOption", cancellationToken, options);
    }

    /// <summary>
    ///     This method purges completed/error/removed downloads to free memory.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task PurgeDownloadResultAsync(CancellationToken cancellationToken = default)
    {
        await _requests.GetRequestAsync("aria2.purgeDownloadResult", cancellationToken);
    }

    /// <summary>
    ///     This method removes a completed/error/removed download denoted by gid from memory.
    /// </summary>
    /// <param name="gid">The GID of the download.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RemoveDownloadResultAsync(String gid, CancellationToken cancellationToken = default)
    {
        await _requests.GetRequestAsync("aria2.removeDownloadResult", cancellationToken, gid);
    }

    public async Task<SessionResult> GetSessionInfo(CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<SessionResult>("aria2.getSessionInfo", cancellationToken);
    }

    /// <summary>
    ///     This method shuts down aria2.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        await _requests.GetRequestAsync("aria2.shutdown", cancellationToken);
    }

    /// <summary>
    ///     This method shuts down aria2(). This method behaves like :func:'aria2.shutdown` without performing any actions
    ///     which take time, such as contacting BitTorrent trackers to unregister downloads first.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ForceShutdownAsync(CancellationToken cancellationToken = default)
    {
        await _requests.GetRequestAsync("aria2.forceShutdown", cancellationToken);
    }

    /// <summary>
    ///     This method returns global statistics such as the overall download and upload speeds.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<GlobalStatResult> GetGlobalStatAsync(CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<GlobalStatResult>("aria2.getGlobalStat", cancellationToken);
    }

    /// <summary>
    ///     This method saves the current session to a file specified by the --save-session option.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IList<String>> SaveSessionAsync(CancellationToken cancellationToken = default)
    {
        return await _requests.GetRequestAsync<List<String>>("aria2.saveSession", cancellationToken);
    }
}