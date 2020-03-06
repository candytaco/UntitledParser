using UntitledParser.Parser.Components.Abstract;
using UntitledParser.Utils;

namespace UntitledParser.Parser.Components.Messages {
	
	public class SvcPaintMapData : DemoMessage {

		private BitStreamReader _data;
		public BitStreamReader Data => _data.FromBeginning();
		
		
		public SvcPaintMapData(SourceDemo demoRef, BitStreamReader reader) : base(demoRef, reader) {}
		
		
		internal override void ParseStream(BitStreamReader bsr) {
			_data = bsr.SubStream(bsr.ReadUInt());
			SetLocalStreamEnd(bsr);
		}
		

		internal override void WriteToStreamWriter(BitStreamWriter bsw) {
			throw new System.NotImplementedException();
		}


		internal override void AppendToWriter(IndentedWriter iw) {
			iw += $"data length in bits: {_data.BitLength}";
		}
	}
}