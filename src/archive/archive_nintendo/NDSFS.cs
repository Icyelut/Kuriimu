﻿using System.Collections.Generic;
using System.IO;
using Kontract.Interface;
using Kontract.IO;

namespace archive_nintendo.NDSFS
{
    public sealed class NDS
    {
        public List<NDSFileInfo> Files = new List<NDSFileInfo>();
        Stream _stream = null;

        RomHeader header;
        Banner banner;
        sFAT[] fat;
        sFolder fnt;

        public NDS(Stream input)
        {
            _stream = input;
            using (BinaryReaderX br = new BinaryReaderX(input, true))
            {
                //Header
                header = new RomHeader(br.BaseStream);

                //Banner
                banner = new Banner(br.BaseStream, header.bannerOffset);

                //FAT
                fat = NdsfsSupport.ReadFAT(br.BaseStream, header.FAToffset, header.FATsize);

                //FNT
                fnt = NdsfsSupport.ReadFNT(br.BaseStream, header.fileNameTableOffset, header.fileNameTableSize, fat);

                //System files
                fnt.folders.Add(NdsfsSupport.AddSystemFiles(br.BaseStream, fat, fat.Length, fnt.id + 0xF000, header));

                //FileData
                FetchFiles(br.BaseStream, fnt);
            }
        }

        public void FetchFiles(Stream input, sFolder fnt, string path="")
        {
            if (fnt.files is List<sFile>)
            {
                for (int i=0;i< fnt.files.Count;i++)
                {
                    Files.Add(new NDSFileInfo
                    {
                        State = ArchiveFileState.Archived,
                        FileName = path + fnt.files[i].name,
                        FileData = new SubStream(input, fnt.files[i].offset, fnt.files[i].size),
                        entry = fnt.files[i]
                    });
                }
            }

            if (fnt.folders is List<sFolder>)
            {
                for (int i=0;i< fnt.folders.Count;i++)
                {
                    FetchFiles(input, fnt.folders[i], path + "/" + fnt.folders[i].name);
                }
            }
        }

        public void Save(Stream output)
        {
            using (BinaryWriterX bw = new BinaryWriterX(output))
            {

            }
        }

        public void Close()
        {
            _stream?.Dispose();
            foreach (var afi in Files)
                if (afi.State != ArchiveFileState.Archived)
                    afi.FileData?.Dispose();
            _stream = null;
        }
    }
}
