using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class MpTauntEarned : UserMessage {

		public string TauntName;
		public bool AwardSilently;


		public MpTauntEarned(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			TauntName = bsr.ReadNullTerminatedString();
			AwardSilently = bsr.ReadBool();
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"taunt name: {TauntName}");
			pw.Append($"award silently: {AwardSilently}");
		}
		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("MPTauntEarned");
			//TODO: this
			parent.Add(thisElement);
		}
	}
}
