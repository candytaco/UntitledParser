using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;

namespace DemoParser.Parser.Components.Messages {

	public class SvcMenu : DemoMessage {

		public ushort MenuType;
		private BitStreamReader _data;
		public BitStreamReader Data => _data.FromBeginning(); // todo


		public SvcMenu(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			MenuType = bsr.ReadUShort();
			uint dataLen = bsr.ReadUInt();
			_data = bsr.ForkAndSkip((int)dataLen);
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"menu type: {MenuType}");
			pw.Append($"data length in bits: {_data.BitLength}");
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("SvcMenu");
			//TODO: this
			parent.Add(thisElement);
		}
		
	}
}
