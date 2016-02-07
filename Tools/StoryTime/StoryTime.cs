using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common;

namespace StoryTime
{
    public class StoryTime
    {
        static void Main(string[] args)
        {
            ExplorationSpace space = Common.ExternalIO.LoadSpace(
                ExternalIO.WorldDataPath(),
                ExternalIO.GraphDataPath());

            if (space != null)
            {
                string stories = Storyteller.Run(space);
                System.IO.File.WriteAllText(
                    ExternalIO.StoryDataPath(), 
                    stories);
                Console.WriteLine(
                    "Wrote stories to " 
                    + ExternalIO.StoryDataPath());
            }

            Console.WriteLine("Press any key to continue . . .");
            Console.ReadKey();
        }
    }
}
