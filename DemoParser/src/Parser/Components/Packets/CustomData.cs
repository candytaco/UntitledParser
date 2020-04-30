#nullable enable
using System;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Parser.Components.Packets.CustomDataTypes;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Packets {
	
	/// <summary>
	/// Contains a single custom game message.
	/// </summary>
	public class CustomData : DemoPacket {

		public CustomDataType DataType;
		public CustomDataMessage DataMessage;


		public CustomData(SourceDemo demoRef, BitStreamReader reader, int tick) : base(demoRef, reader, tick) {}


		internal override void ParseStream(BitStreamReader bsr) {
			DataType = (CustomDataType)bsr.ReadSInt();
			uint size = bsr.ReadUInt();
			DataMessage = CustomDataFactory.CreateCustomDataMessage(DemoRef, bsr.SubStream(size * 8), DataType);
			try {
				DataMessage.ParseOwnStream();
			} catch (Exception e) {
				DemoRef.AddError($"error while parsing custom data of type: {DataType}... {e.Message}");
				DataMessage = new UnknownCustomDataMessage(DemoRef, DataMessage.Reader);
				DataMessage.ParseOwnStream();
			}
			bsr.SkipBytes(size);
			SetLocalStreamEnd(bsr);
		}
		

		internal override void WriteToStreamWriter(BitStreamWriter bsw) {
			throw new NotImplementedException();
		}


		public override void AppendToWriter(IndentedWriter iw) {
			iw.Append($"type: {DataType}");
			iw.FutureIndent++;
			iw.AppendLine();
			DataMessage.AppendToWriter(iw);
			iw.FutureIndent--;
		}
	}
}