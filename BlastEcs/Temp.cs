//using BlastEcs.Collections;

//namespace BlastEcs;

//sealed class Q
//{

//    public void Iterate(EcsWorld world, Term[][] termArrays)
//    {
//        //Step 1:
//        //Find all possible archetypes based on 
//        //fixed terms (All components/Tags and all wildcards)


//        //Step 2:
//        //Perform Backtracking algo on returned results
//        //
//        for (int i = 0; i < termArrays.Length; i++)
//        {
//            var terms = termArrays[i];
//            BitMask archetypes = new();
//            for (int j = 0; j < terms.Length; j++)
//            {
//                var term = terms[j];
//                world.GetArchetypesWith(term, archetypes, j == 0);
//                if (term.Target == OperationTarget.)
//                {

//                }
//            }
//        }
//    }


//}