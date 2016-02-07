using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ExternalIO
    {
        public const string DATA_PATH = "C:\\Users\\Alex\\Documents\\DRZ\\devl\\Unity\\Core\\Assets\\Data\\";
        public const string WORLD_FILE_NAME = "bankVideo.dat";
        public const string GRAPH_FILE_NAME = "bankVideoGraph.dat";
        public const string STORY_FILE_NAME = "bankVideoStory.txt";
        public const string DEBUG_FILE_NAME = "bankVideoDebug.txt";

        public static string WorldDataPath()
        {
            return DATA_PATH + WORLD_FILE_NAME;
        }

        public static string GraphDataPath()
        {
            return DATA_PATH + GRAPH_FILE_NAME;
        }

        public static string StoryDataPath()
        {
            return DATA_PATH + STORY_FILE_NAME;
        }

        public static string DebugDataPath()
        {
            return DATA_PATH + DEBUG_FILE_NAME;
        }

        public static ExplorationSpace LoadSpace(
            string worldDataPath,
            string graphDataPath)
        {
            WorldState worldState;
            EventDescriptor[] evtDescs;
            bool loadedWorld =
                DataIO.LoadWorldXML(
                    worldDataPath,
                    out worldState,
                    out evtDescs);

            if (loadedWorld == false)
            {
                Console.WriteLine(
                    "Failed to load world file " + worldDataPath);
                return null;
            }

            Console.WriteLine("Loaded " + evtDescs.Length + " events...");

            PseudoEventLibrary pseudoLib =
                new PseudoEventLibrary(evtDescs);
            ExplorationSpace space =
                DataIO.LoadGraphBinary(
                    graphDataPath,
                    pseudoLib);

            if (space == null)
            {
                Console.WriteLine(
                    "Failed to load graph file " + graphDataPath);
                return null;
            }

            Console.WriteLine("Loaded " + space.Nodes.Count + " nodes...");
            return space;
        }
    }
}
