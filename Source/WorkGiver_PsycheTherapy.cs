using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Psyche
{
    public class WorkGiver_PsycheTherapy : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            IReadOnlyList<Pawn> pawns = pawn.Map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].CurJobDef == PsycheDefOf.Psyche_SeekTherapy)
                {
                    yield return pawns[i];
                }
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn patient) || patient == pawn)
            {
                return false;
            }

            if (patient.CurJobDef != PsycheDefOf.Psyche_SeekTherapy)
            {
                return false;
            }

            if (!PsycheUtility.IsTracked(patient) || !PsycheTherapy.NeedsCare(patient))
            {
                return false;
            }

            return pawn.CanReserveAndReach(patient, PathEndMode.Touch, forced ? Danger.Deadly : Danger.Some, 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(PsycheDefOf.Psyche_AdministerTherapy, (Pawn)t);
        }
    }
}
