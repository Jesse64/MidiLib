using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MidiLib.Messages
{
    public enum Keys : byte
    {
        VLV = 00,
        NoteOff = 0x80,
        NoteOn = 0x90,
        AfterTouch = 0xA0,
        Controller = 0xB0,
        Patch = 0xC0,
        Pressure = 0xD0,
        PitchBend = 0xE0,
        Meta = 0xF0
    }

    abstract public class Message
    {
        /// <summary>
        /// Channel number
        /// </summary>
        public byte Channel;

        /// <summary>
        /// Get parameter as object
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract object GetParameter(int index);

        /// <summary>
        /// Get parameter as integer if possible.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Returns 0 when not convertable to integer</returns>
        public abstract int GetParameterInt(int index);

        public abstract Keys GetKey();

        /// <summary>
        /// Writes a message
        /// </summary>
        /// <param name="binaryWriter"></param>
        /// <param name="channel"></param>
        internal abstract void Write(BinaryWriterBE binaryWriter, byte channel);
    }

    public class VLV : Message
    {
        private int value;

        public VLV(int integer)
        {
            value = integer;
        }

        internal VLV(BinaryReaderBE binaryReader)
        {
            value = 0;
            value = ToInt(readVLV(binaryReader));
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return value;
            else return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return value;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.VLV;
        }

        public void Add(int value)
        {
            this.value += value;
        }

        public static int ToInt(int vlv)
        {
            int number;

            number = (vlv & 0x7F000000) >> 24;
            number = (number << 7) + ((vlv & 0x7F0000) >> 16);
            number = (number << 7) + ((vlv & 0x7F00) >> 8);
            number = (number << 7) + (vlv & 0x7F);

            return number;
        }

        public static int ToVlV(int integer)
        {
            int buffer = integer & 0x7F;

            while (Convert.ToBoolean(integer >>= 7))
            {
                buffer <<= 8;
                buffer |= ((integer & 0x7F) | 0x80);
            }

            return buffer;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {
            int buffer = ToVlV(value);

            while (true)
            {
                binaryWriter.Write(Convert.ToByte((buffer << 24) >> 24 & 0xFF));
                if (Convert.ToBoolean(buffer & 0x80)) buffer >>= 8;
                else break;
            }
        }

        private int readVLV(BinaryReaderBE binaryReader)
        {
            int delta = 0;

            while (true)
            {
                delta += binaryReader.ReadByte();
                if ((delta & 0x80) == 0x80)
                {
                    delta <<= 8;
                }
                else break;
            }

            return delta;
        }
    }

    public class NoteOn : Message
    {
        private byte note;
        private byte velocity;

        public NoteOn(byte note, byte velocity, byte channel)
        {
            this.Channel = channel;
            this.note = note;
            this.velocity = velocity;
        }

        public NoteOn(byte note, byte velocity)
        {
            this.Channel = 0xFF;
            this.note = note;
            this.velocity = velocity;
        }

        internal NoteOn(BinaryReaderBE binaryReader, int channel)
        {
            this.Channel = (byte)channel;
            note = binaryReader.ReadByte();
            velocity = binaryReader.ReadByte();
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return note;
            else if (index == 1) return velocity;
            return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return note;
            else if (index == 1) return velocity;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.NoteOn;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {
            if(this.Channel == 0xFF) binaryWriter.Write(Convert.ToByte((byte)Keys.NoteOn | channel));
            else binaryWriter.Write(Convert.ToByte((byte)Keys.NoteOn | this.Channel));
            binaryWriter.Write(note);
            binaryWriter.Write(velocity);
        }
    }

    public class NoteOff : Message
    {
        private byte note;
        private byte velocity;

        public NoteOff(byte note, byte velocity, byte channel)
        {
            this.Channel = channel;
            this.note = note;
            this.velocity = velocity;
        }

        public NoteOff(byte note, byte velocity)
        {
            this.Channel = 0xFF;
            this.note = note;
            this.velocity = velocity;
        }

        internal NoteOff(BinaryReaderBE binaryReader, int channel)
        {
            this.Channel = (byte)channel;
            note = binaryReader.ReadByte();
            velocity = binaryReader.ReadByte();
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return note;
            else if (index == 1) return velocity;
            return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return note;
            else if (index == 1) return velocity;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.NoteOff;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {
            if (this.Channel == 0xFF) binaryWriter.Write(Convert.ToByte((byte)Keys.NoteOff | channel));
            else binaryWriter.Write(Convert.ToByte((byte)Keys.NoteOff | this.Channel));
            binaryWriter.Write(note);
            binaryWriter.Write(velocity);
        }
    }

    public class AfterTouch : Message
    {
        private byte note;
        private byte touch;

        public AfterTouch(byte note, byte touch, byte channel)
        {
            this.Channel = channel;
            this.note = note;
            this.touch = touch;
        }

        public AfterTouch(byte note, byte touch)
        {
            this.Channel = 0xFF;
            this.note = note;
            this.touch = touch;
        }

        internal AfterTouch(BinaryReaderBE binaryReader, int channel)
        {
            this.Channel = (byte)channel;
            note = binaryReader.ReadByte();
            touch = binaryReader.ReadByte();
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return note;
            else if (index == 1) return touch;
            return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return note;
            else if (index == 1) return touch;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.AfterTouch;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {

            if (this.Channel == 0xFF) binaryWriter.Write(Convert.ToByte((byte)Keys.AfterTouch | channel));
            else binaryWriter.Write(Convert.ToByte((byte)Keys.AfterTouch | this.Channel));
            binaryWriter.Write(note);
            binaryWriter.Write(touch);
        }
    }

    public enum ControllerType : byte
    {
        Bank = 0x00,
        Modulation = 0x01,
        Breath = 0x02,
        Foot = 0x04,
        Portamento = 0x05,
        Volume = 0x07,
        Balance = 0x08,
        Pan = 0x0A
    }

    public class Controller : Message
    {
        private ControllerType type;
        private byte value;

        public Controller(ControllerType type, byte value, byte channel)
        {
            this.Channel = channel;
            this.type = type;
            this.value = value;
        }

        public Controller(ControllerType type, byte value)
        {
            this.Channel = 0xFF;
            this.type = type;
            this.value = value;
        }

        internal Controller(BinaryReaderBE binaryReader, int channel)
        {
            this.Channel = (byte)channel;
            type = (ControllerType)binaryReader.ReadByte();
            value = binaryReader.ReadByte();
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return type;
            else if (index == 1) return value;
            return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return (int)type;
            else if (index == 1) return value;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.Controller;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {
            if (this.Channel == 0xFF) binaryWriter.Write(Convert.ToByte((byte)Keys.Controller | channel));
            else binaryWriter.Write(Convert.ToByte((byte)Keys.Controller | this.Channel));
            binaryWriter.Write((byte)type);
            binaryWriter.Write(value);
        }
    }

    public class Patch : Message
    {
        private byte instrument;

        public Patch(byte instrument, byte channel)
        {
            this.Channel = channel;
            this.instrument = instrument;
        }

        public Patch(byte instrument)
        {
            this.Channel = 0xFF;
            this.instrument = instrument;
        }

        internal Patch(BinaryReaderBE binaryReader, int channel)
        {
            this.Channel = (byte)channel;
            instrument = binaryReader.ReadByte();
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return instrument;
            return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return instrument;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.Patch;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {
            if (this.Channel == 0xFF) binaryWriter.Write(Convert.ToByte((byte)Keys.Patch | channel));
            else binaryWriter.Write(Convert.ToByte((byte)Keys.Patch | this.Channel));
            binaryWriter.Write(instrument);
        }
    }

    public class Pressure : Message
    {
        private byte pressure;

        public Pressure(byte pressure, byte channel)
        {
            this.Channel = channel;
            this.pressure = pressure;
        }

        public Pressure(byte pressure)
        {
            this.Channel = 0xFF;
            this.pressure = pressure;
        }

        internal Pressure(BinaryReaderBE binaryReader, int channel)
        {
            this.Channel = (byte)channel;
            pressure = binaryReader.ReadByte();
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return pressure;
            return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return pressure;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.Pressure;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {
            if (this.Channel == 0xFF) binaryWriter.Write(Convert.ToByte((byte)Keys.Pressure | channel));
            else binaryWriter.Write(Convert.ToByte((byte)Keys.Pressure | this.Channel));
            binaryWriter.Write(pressure);
        }
    }

    public class PitchBend : Message
    {
        private byte lsb;
        private byte msb;

        public PitchBend(byte lsb, byte msb, byte channel)
        {
            this.Channel = channel;
            this.lsb = lsb;
            this.msb = msb;
        }

        public PitchBend(byte lsb, byte msb)
        {
            this.Channel = 0xFF;
            this.lsb = lsb;
            this.msb = msb;
        }

        internal PitchBend(BinaryReaderBE binaryReader, int channel)
        {
            this.Channel = (byte)channel;
            lsb = binaryReader.ReadByte();
            msb = binaryReader.ReadByte();
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return lsb;
            else if (index == 1) return msb;
            return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return lsb;
            else if (index == 1) return msb;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.PitchBend;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {
            if (this.Channel == 0xFF) binaryWriter.Write(Convert.ToByte((byte)Keys.PitchBend | channel));
            else binaryWriter.Write(Convert.ToByte((byte)Keys.PitchBend | this.Channel));
            binaryWriter.Write(lsb);
            binaryWriter.Write(msb);
        }
    }

    public class Meta : Message
    {
        private byte type;
        private byte length;
        private byte[] data;

        public Meta(byte type, byte[] data)
        {
            if (data.Length > 255) throw new ArgumentException("Length of data is over 255. Make sure the length is less than 255", "data");

            this.length = (byte)data.Length;
            this.type = type;
            this.data = data;
        }

        public Meta(byte type, string str)
        {
            if (str.Length > 255) throw new ArgumentException("Length of str is over 255. Make sure the length is less than 255", "str");

            this.length = (byte)str.Length;
            this.type = type;
            this.data = getBytes(str);
        }

        public Meta(byte type, int data)
        {
            if (type == 0x51)
            {
                this.length = 3;
                this.type = type;
                this.data = new byte[] { Convert.ToByte((data & 0x00FF0000) >> 16), 
                                        Convert.ToByte((data & 0x0000FF00) >> 8),
                                        Convert.ToByte(data & 0x000000FF) };
            }
            else
            {
                this.length = 4;
                this.type = type;
                this.data = BitConverter.GetBytes(data);
            }
        }

        private byte[] getBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        internal Meta(BinaryReaderBE binaryReader)
        {
            type = binaryReader.ReadByte();
            length = binaryReader.ReadByte();
            data = binaryReader.ReadBytes(length);
        }

        public override object GetParameter(int index)
        {
            if (index == 0) return type;
            else if (index == 1) return length;
            else if (index == 2) return data;
            return null;
        }

        public override int GetParameterInt(int index)
        {
            if (index == 0) return type;
            else if (index == 1) return length;
            else return 0;
        }

        public override Keys GetKey()
        {
            return Keys.Meta;
        }

        internal override void Write(BinaryWriterBE binaryWriter, byte channel)
        {
            binaryWriter.Write(Convert.ToByte((byte)Keys.Meta | channel));
            binaryWriter.Write(type);
            binaryWriter.Write((byte)data.Length);
            binaryWriter.Write(data);
        }
    }
}
