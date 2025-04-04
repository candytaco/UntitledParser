using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages {

	public class SvcEntityMessage : DemoMessage {

		public uint EntityIndex;
		public uint ClassId;
		private BitStreamReader _data; // todo
		public BitStreamReader Data => _data.FromBeginning();


		public SvcEntityMessage(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			EntityIndex = bsr.ReadUInt(DemoInfo.MaxEdictBits);
			ClassId = bsr.ReadUInt(9);
			_data = bsr.ForkAndSkip((int)bsr.ReadUInt(11));
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"entity index: {EntityIndex}");
			pw.AppendLine($"class ID: {ClassId}");
			pw.Append($"data length in bits: {_data.BitLength}");
		}

		public override void XMLWrite(XElement parent)
		{
			parent.Add(new XElement("SvcEntityMessage"));
			// TODO: this
		}
	}
}
