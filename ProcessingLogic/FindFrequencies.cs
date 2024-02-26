using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessingLogic
{
    public class FindFrequencies
    {
        public List<Tuple<int, float>> FindBlock(List<float[]> buffer, int start, int end, int bottom, int top)
        {
            var results = new List<Tuple<int, float>>();
            //for each row
            for (int row = bottom; row < top; row += 2)
            {
                //add all values
                float strength = 0;
                for (int column = start; column < end; column++)
                {
                    var val = buffer[column][row];
                    var val2 = buffer[column][row + 1];
                    if (val > 0.05 || val2 > 0.05)
                        strength++;
                }
                if (strength > (end - start) * .75)
                {
                    results.Add(new Tuple<int, float>(row, strength));
                }
            }

            return results;
        }
    }
}
