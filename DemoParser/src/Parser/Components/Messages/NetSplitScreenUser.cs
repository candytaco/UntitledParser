using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages {

	public class NetSplitScreenUser : DemoMessage {

		public bool Bool;


		public NetSplitScreenUser(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Bool = bsr.ReadBool();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"unknown bool: {Bool}");
		}

		public override void XMLWrite(XElement parent)
		{
			parent.Add(new XElement("NetSplitScreenUser", Bool));
		}
	}
}
