#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DemoParser.Parser.Components.Messages;

namespace DemoParser.Parser.HelperClasses.EntityStuff {
    
    // like C_StringTablesManager, keeps a local copy of the baselines that can be updated and changed whenever
    public class C_BaseLines {

        /*
        The baseline is an array of all server classes and their default properties.
        However, a server class will not appear in the demo unless it is actually used in the level, 
        therefore any element of the baseline will be null until that corresponding slot is initialized.
        In addition, once initialized, apparently not every property for a given class
        is given a default value. (I'm guessing this is for properties that are known to be set to
        something as soon as the entity is created.)
        */
        public (ServerClass? serverClass, EntityProperty?[]? entityProperties)[] ClassBaselines;
        private readonly SourceDemo _demoRef;


        public C_BaseLines(int maxServerClasses, SourceDemo demoRef) {
            _demoRef = demoRef;
            ClearBaseLineState(maxServerClasses);
        }


        public void ClearBaseLineState(int maxServerClasses) {
            ClassBaselines = new (ServerClass? serverClass, EntityProperty?[]? entityProperties)[maxServerClasses];
        }


        // either updated from the instance baseline in string tables or the dynamic baseline in the entities message
        // this might be wrong, it's possible all updates are stored to a new baseline, so effectively only the last one matters
        public void UpdateBaseLine(
            [NotNull] ServerClass serverClass, 
            [NotNull] IEnumerable<(int propIndex, EntityProperty prop)> props,
            int entPropCount) 
        {
            int i = serverClass.DataTableId;
            if (ClassBaselines[i] == default) { // just init that class slot, i'll write the properties next anyway
                ClassBaselines[i].serverClass = serverClass;
                ClassBaselines[i].entityProperties = new EntityProperty[entPropCount];
            }
            
            // update the slot
            foreach ((int propIndex, EntityProperty from) in props) {
                ref EntityProperty to = ref ClassBaselines[i].entityProperties[propIndex];
                if (to == null)
                    to = from.CopyProperty();
                else
                    from.CopyPropertyTo(to); // I think this is only necessary for arrays, where old elements may be kept
            }
        }


        internal Entity EntFromBaseLine(ServerClass serverClass, uint serial) {
            
            // assume classes[ID] is valid
            int classIndex = serverClass.DataTableId; 
            ref EntityProperty?[] props = ref ClassBaselines[classIndex].entityProperties;

            // I need this because for now I cannot parse string tables that are encoded with dictionaries,
            // so in anything that isn't 3420 I cannot parse baseline table updates outside of the string table packet.
            if (props == null) {
                _demoRef.AddError("attempted to create entity from a non-existing baseline, " +
                                  $"creating new empty baseline for class: ({serverClass.ToString()})");
                
                List<FlattenedProp> fProps = _demoRef.DataTableParser.FlattenedProps[classIndex].flattenedProps;
                props = new EntityProperty[fProps.Count];
                ClassBaselines[classIndex].serverClass = serverClass;
            }
            
            EntityProperty[] newEnt = new EntityProperty[props.Length];
            for (int i = 0; i < newEnt.Length; i++) {
                if (props[i] == null)
                    newEnt[i] = null;
                else
                    newEnt[i] = props[i].CopyProperty();
            }

            return new Entity(ClassBaselines[classIndex].serverClass, newEnt, serial);
        }
    }
}