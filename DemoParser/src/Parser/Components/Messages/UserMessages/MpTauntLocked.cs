using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class MpTauntLocked : UserMessage {

		public string TauntName;


		public MpTauntLocked(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			TauntName = bsr.ReadNullTerminatedString();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.Append($"taunt name: {TauntName}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("MPTauntLocked");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
