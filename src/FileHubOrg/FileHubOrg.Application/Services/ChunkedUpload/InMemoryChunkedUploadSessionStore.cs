using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FileHubOrg.Application.Interfaces;
using FileHubOrg.Application.Models.ChunkedUpload;

namespace FileHubOrg.Application.Services.ChunkedUpload
{
    // NOTE: For production, replace with a distributed cache/DB store.
    public class InMemoryChunkedUploadSessionStore : IChunkedUploadSessionStore
    {
        private readonly ConcurrentDictionary<Guid, UploadSession> _sessions = new();

        public Task<UploadSession> CreateSessionAsync(UploadSession session)
        {
            if (session.UploadId == Guid.Empty)
                session.UploadId = Guid.NewGuid();

            _sessions[session.UploadId] = session;
            return Task.FromResult(session);
        }

        public Task<UploadSession?> GetSessionAsync(Guid uploadId)
        {
            _sessions.TryGetValue(uploadId, out var session);
            return Task.FromResult(session);
        }

        public Task UpdateSessionAsync(UploadSession session)
        {
            _sessions[session.UploadId] = session;
            return Task.CompletedTask;
        }

        public Task DeleteSessionAsync(Guid uploadId)
        {
            _sessions.TryRemove(uploadId, out _);
            return Task.CompletedTask;
        }
    }
}

