using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MidiLib
{
    public class Midi
    {
        private List<Track> tracks;

        private short type;

        public short TrackCount { get { return (short)tracks.Count; } }

        public short PPQ { get; set; }

        public Midi()
        {
            this.type = 1;
            this.PPQ = 0x00F0;
            this.tracks = new List<Track>();
        }
        
        public Midi(string fileName)
        {
            load(new BinaryReaderBE(File.OpenRead(fileName)));
        }

        public Midi(FileStream fileStream)
        {
            load(new BinaryReaderBE(fileStream));
        }

        internal Midi(BinaryReaderBE binaryReader)
        {
            load(binaryReader);
        }

        public void WriteMidi(string fileName)
        {
            write(new BinaryWriterBE(File.OpenWrite(fileName)));
        }

        public void WriteMidi(BinaryWriter binaryWriter)
        {
            write(new BinaryWriterBE(binaryWriter.BaseStream));
        }

        /// <summary>
        /// Add track to the midi
        /// </summary>
        /// <param name="track"></param>
        public void AddTrack(Track track)
        {
            tracks.Add(track);
        }

        /// <summary>
        /// Get a track by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Track GetTrack(int index)
        {
            try
            {
                return tracks[index];
            }
            catch (IndexOutOfRangeException e)
            {
                throw e;
            }
        }

        private void load(BinaryReaderBE binaryReader)
        {
            tracks = new List<Track>();

            checkMthd(binaryReader.ReadBytes(4));

            if (binaryReader.ReadInt32() != 6) throw new FormatException("Wrong header size");

            type = binaryReader.ReadInt16();
            short trackCount = binaryReader.ReadInt16();
            PPQ = binaryReader.ReadInt16();

            for (int i = 0; i < trackCount; i++) tracks.Add(new Track(binaryReader));
        }

        private void checkMthd(byte[] bytes)
        {
            if (bytes[0] == 0x4D &&
                bytes[1] == 0x54 &&
                bytes[2] == 0x68 &&
                bytes[3] == 0x64)
                return;
            else throw new FormatException("File is not a recognized MIDI format");
        }

        private void write(BinaryWriterBE binaryWriter)
        {
            binaryWriter.Write(0x4D546864);
            binaryWriter.Write(6);
            binaryWriter.Write(type);
            binaryWriter.Write(Convert.ToInt16(tracks.Count));
            binaryWriter.Write(PPQ);

            for (int i = 0; i < tracks.Count; i++)
            {
                tracks[i].writeTrack(binaryWriter, (byte)i);
            }
        }
    }
}
