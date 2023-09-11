using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Dynamic.Io;

namespace DotRecast.Recast.Toolset.Tools
{
    public class RcDynamicUpdateTool : IRcToolable
    {
        public string GetName()
        {
            return "Dynamic Updates";
        }

        public VoxelFile Load(string filename, IRcCompressor compressor)
        {
            using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            VoxelFileReader reader = new VoxelFileReader(compressor);
            VoxelFile voxelFile = reader.Read(br);

            return voxelFile;
        }

        public void Save(string filename, VoxelFile voxelFile, bool compression, IRcCompressor compressor)
        {
            using var fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);
            using var bw = new BinaryWriter(fs);
            VoxelFileWriter writer = new VoxelFileWriter(compressor);
            writer.Write(bw, voxelFile, compression);
        }
    }
}