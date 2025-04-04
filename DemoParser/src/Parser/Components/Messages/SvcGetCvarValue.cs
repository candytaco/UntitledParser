using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages {

	public class SvcGetCvarValue : DemoMessage {

		public int Cookie;
		public string CvarName;


		public SvcGetCvarValue(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Cookie = bsr.ReadSInt();
			CvarName = bsr.ReadNullTerminatedString();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"cookie: {Cookie}");
			pw.Append($"cvar name: {CvarName}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("SvcGetCvarValue");
			//TODO: this
			parent.Add(thisElement);
		}
		
	}
}
