using System.Linq;
using DemoParser.Parser.Components.Messages;

namespace DemoParser.Parser.HelperClasses.EntityStuff {
    
    public class Entity {
		
		public readonly ServerClass ServerClass;
		public readonly uint Serial;
		public readonly EntityProperty[] Props; // readonly but mutable
		public bool InPvs;
		
		
		// assume props array is deep-copied
		private Entity(ServerClass serverClass, EntityProperty[] props, uint serial, bool inPvs) { 
			ServerClass = serverClass;
			Props = props;
			Serial = serial;
			InPvs = inPvs;
		}
		
		
		public Entity(ServerClass serverClass, EntityProperty[] props, uint serial) 
			: this (serverClass, props, serial, true){}


		public override string ToString() {
			return $"class: {ServerClass.ClassName} ({ServerClass.DataTableName}) [{ServerClass.DataTableId}], serial: {Serial}";
		}


		public Entity Duplicate() => 
			new Entity(ServerClass, Props.Select(property => property?.CopyProperty()).ToArray(), Serial, InPvs);
	}
}