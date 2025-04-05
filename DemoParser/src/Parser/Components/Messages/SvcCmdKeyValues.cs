using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages {


	public class SvcCmdKeyValues : DemoMessage {

		public byte[] Arr; // todo


		public SvcCmdKeyValues(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Arr = bsr.ReadBytes((int)bsr.ReadUInt());
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"array of length: {Arr.Length}");
		}

		public override void XMLWrite(XElement parent)
		{
			parent.Add(new XElement("SvcCommandKeyValues"));
			// TODO: this
		}
	}
}
