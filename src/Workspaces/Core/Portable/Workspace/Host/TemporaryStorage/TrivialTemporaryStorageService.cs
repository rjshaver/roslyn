﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    [ExportWorkspaceService(typeof(ITemporaryStorageService)), Shared]
    internal sealed class TrivialTemporaryStorageService : ITemporaryStorageService
    {
        public ITemporaryStreamStorage CreateTemporaryStreamStorage(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new StreamStorage();
        }

        public ITemporaryTextStorage CreateTemporaryTextStorage(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new TextStorage();
        }

        private class StreamStorage : ITemporaryStreamStorage
        {
            private MemoryStream stream;

            public void Dispose()
            {
            }

            public Stream ReadStream(CancellationToken cancellationToken = default(CancellationToken))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException();
                }

                stream.Position = 0;
                return stream;
            }

            public Task<Stream> ReadStreamAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException();
                }

                stream.Position = 0;
                return Task.FromResult((Stream)stream);
            }

            public void WriteStream(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
            {
                var newStream = new MemoryStream();
                stream.CopyTo(newStream);
                this.stream = newStream;
            }

            public async Task WriteStreamAsync(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
            {
                var newStream = new MemoryStream();
                await stream.CopyToAsync(newStream).ConfigureAwait(false);
                this.stream = newStream;
            }
        }

        private class TextStorage : ITemporaryTextStorage
        {
            private string text;
            private Encoding encoding;

            public void Dispose()
            {
            }

            public SourceText ReadText(CancellationToken cancellationToken = default(CancellationToken))
            {
                return SourceText.From(text, encoding);
            }

            public Task<SourceText> ReadTextAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult(ReadText(cancellationToken));
            }

            public void WriteText(SourceText text, CancellationToken cancellationToken = default(CancellationToken))
            {
                // Decompose the SourceText into it's underlying parts, since we use it as a key
                // into many other caches that don't expect it to be held
                this.text = text.ToString();
                this.encoding = text.Encoding;
            }

            public Task WriteTextAsync(SourceText text, CancellationToken cancellationToken = default(CancellationToken))
            {
                WriteText(text, cancellationToken);
                return SpecializedTasks.EmptyTask;
            }
        }
    }
}
