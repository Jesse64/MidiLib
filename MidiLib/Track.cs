using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MidiLib.Messages;

namespace MidiLib
{
    public class Track
    {
        private List<Message> messages;

        private int length;

        private int curMessage;

        public int Count { get { return messages.Count; } }

        public Track()
        {
            this.length = 0;
            this.curMessage = 0;
            this.messages = new List<Message>();
        }

        internal Track(BinaryReaderBE binaryReader)
        {
            messages = new List<Message>();
            curMessage = 0;

            checkMtrk(binaryReader.ReadBytes(4));

            length = binaryReader.ReadInt32();

            readMessages(binaryReader);
        }

        /// <summary>
        /// Add a vlv and message
        /// </summary>
        /// <param name="vlv"></param>
        /// <param name="message"></param>
        public void AddMessage(VLV vlv, Message message)
        {
            AddVLV(vlv);
            AddMessage(message);
        }

        /// <summary>
        /// Add a message
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(Message message)
        {
            if (messages.Count > 0)
            {
                if (messages[messages.Count - 1].GetKey() != Keys.VLV)
                    messages.Add(new VLV(0));
            }
            else messages.Add(new VLV(0));

            messages.Add(message);
        }

        /// <summary>
        /// Add a vlv
        /// </summary>
        /// <param name="vlv"></param>
        public void AddVLV(VLV vlv)
        {
            if (messages.Count > 0)
            {
                if (messages[messages.Count - 1].GetKey() == Keys.VLV)
                {
                    messages[messages.Count - 1] = new VLV(messages[messages.Count - 1].GetParameterInt(0) + vlv.GetParameterInt(0));
                }
                else messages.Add(vlv);
            }
            else
            {
                messages.Add(vlv);
            }
        }

        /// <summary>
        /// Get the next message
        /// </summary>
        /// <returns></returns>
        public Message Next()
        {
            try
            {
                Message message = messages[curMessage];
                curMessage++;
                return message;
            }
            catch (IndexOutOfRangeException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Get the previous message
        /// </summary>
        /// <returns></returns>
        public Message Previous()
        {
            try
            {
                Message message = messages[curMessage];
                curMessage--;
                return message;
            }
            catch (IndexOutOfRangeException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Check whether this track has any non meta messages
        /// </summary>
        /// <returns>Returns true if a non meta message has been found</returns>
        public bool CheckNonMetaMessages()
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].GetKey() != Keys.Meta && messages[i].GetKey() != Keys.VLV) return true;
            }

            return false;
        }

        /// <summary>
        /// Checks for rests between the current message and a note on
        /// </summary>
        /// <returns></returns>
        public bool CheckRests()
        {
            for (int i = curMessage; i < messages.Count; i++)
            {
                if (messages[i].GetKey() == Keys.NoteOn) return false;

                if (messages[i].GetKey() == Keys.VLV)
                    if (messages[i].GetParameterInt(0) > 0) return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether a message is found before the end of the track
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public bool Check(Keys search)
        {
            for (int i = curMessage; i < messages.Count; i++)
            {
                if (messages[i].GetKey() == search) return true;
            }

            return false;
        }

        /// <summary>
        /// Checks whether a message is found before the given message
        /// </summary>
        /// <param name="search"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public bool Check(Keys search, Keys end)
        {
            for (int i = curMessage; i < messages.Count; i++)
            {
                if (messages[i].GetKey() == search) return true;

                if (messages[i].GetKey() == end) return false;
            }

            return false;
        }

        internal void writeTrack(BinaryWriterBE binaryWriter, byte track)
        {
            binaryWriter.Write(0x4D54726B);

            binaryWriter.Write(0);
            long lenPos = binaryWriter.BaseStream.Position;

            for (int i = 0; i < messages.Count; i++)
            {
                switch (messages[i].GetKey())
                {
                    case Keys.VLV: 
                        messages[i].Write(binaryWriter, track);
                        break;
                    case Keys.NoteOff: 
                        messages[i].Write(binaryWriter, track);
                        break;
                    case Keys.NoteOn:
                        messages[i].Write(binaryWriter, track);
                        break;
                    case Keys.AfterTouch:
                        messages[i].Write(binaryWriter, track);
                        break;
                    case Keys.Controller:
                        messages[i].Write(binaryWriter, track);
                        break;
                    case Keys.Patch:
                        messages[i].Write(binaryWriter, track);
                        break;
                    case Keys.Pressure:
                        messages[i].Write(binaryWriter, track);
                        break;
                    case Keys.PitchBend:
                        messages[i].Write(binaryWriter, track);
                        break;
                    case Keys.Meta:
                        messages[i].Write(binaryWriter, 0x0F);
                        break;
                }
            }

            if (messages[messages.Count - 1].GetKey() != Keys.Meta) throw new Exception("Track does not end with an end command");

            long endPos = binaryWriter.BaseStream.Position;

            int trackLength = (int)binaryWriter.BaseStream.Position - (int)lenPos;
            binaryWriter.BaseStream.Position = lenPos - 4;
            binaryWriter.Write(trackLength);

            binaryWriter.BaseStream.Position = endPos;
        }

        private void readMessages(BinaryReaderBE binaryReader)
        {
            long curLength = (long)binaryReader.BaseStream.Position + length;

            while(binaryReader.BaseStream.Position <  curLength)
            {
                messages.Add(new VLV(binaryReader));
                
                //TODO implement running status

                byte status = binaryReader.ReadByte();

                switch ((Keys)(status & 0xF0))
                {
                    case Keys.NoteOff: messages.Add(new NoteOff(binaryReader, status & 0x0F)); break;
                    case Keys.NoteOn: messages.Add(new NoteOn(binaryReader, status & 0x0F)); break;
                    case Keys.AfterTouch: messages.Add(new AfterTouch(binaryReader, status & 0x0F)); break;
                    case Keys.Controller: messages.Add(new Controller(binaryReader, status & 0x0F)); break;
                    case Keys.Patch: messages.Add(new Patch(binaryReader, status & 0x0F)); break;
                    case Keys.Pressure: messages.Add(new Pressure(binaryReader, status & 0x0F)); break;
                    case Keys.PitchBend: messages.Add(new PitchBend(binaryReader, status & 0x0F)); break;
                    case Keys.Meta: messages.Add(new Meta(binaryReader)); break;
                    default: throw new NotImplementedException("Unknown message: "+status);
                }
            }
        }

        private void checkMtrk(byte[] bytes)
        {
            if (bytes[0] == 0x4D &&
                bytes[1] == 0x54 &&
                bytes[2] == 0x72 &&
                bytes[3] == 0x6B)
                return;
            else throw new FormatException("Not a recognized track");
        }
    }
}
