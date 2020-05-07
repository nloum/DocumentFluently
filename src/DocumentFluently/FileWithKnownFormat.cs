using System;
using System.Threading.Tasks;
using IoFluently;

namespace DocumentFluently
{
    public class FileWithKnownFormat<TReader>
    {
        public AbsolutePath Path { get; }
        
        private Func<AbsolutePath, Task<TReader>> _read;

        public FileWithKnownFormat(AbsolutePath path, Func<AbsolutePath, Task<TReader>> read)
        {
            Path = path;
            _read = read;
        }

        public Task<TReader> Read()
        {
            return _read(Path);
        }
    }

    public class FileWithKnownFormat<TReader, TWriter> : FileWithKnownFormat<TReader>
    {
        private Func<AbsolutePath, TWriter, Task> _write;

        public FileWithKnownFormat(AbsolutePath path, Func<AbsolutePath, Task<TReader>> read, Func<AbsolutePath, TWriter, Task> write) : base(path, read)
        {
            _write = write;
        }

        public Task Write(TWriter writer)
        {
            return _write(Path, writer);
        }
    }
}