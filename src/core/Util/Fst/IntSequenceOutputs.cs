﻿using System;
using System.Diagnostics;

namespace Lucene.Net.Util.Fst
{

    public class IntSequenceOutputs : Outputs<IntsRef>
    {
        private static readonly IntsRef NO_OUTPUT;
        private static readonly IntSequenceOutputs _singleton;

        static IntSequenceOutputs()
        {
            NO_OUTPUT = new IntsRef();
            _singleton = new IntSequenceOutputs();
        }

        private IntSequenceOutputs() {} // can't construct

        public GetSingleton() { return _singleton; }

        public override IntsRef Add(IntsRef prefix, IntsRef output)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            if (output == null) throw new ArgumentNullException("output");

            if (prefix == NO_OUTPUT) return output;
            if (output == NO_OUTPUT) return prefix;

            // TODO: assert correct here?
            Debug.Assert(prefix.length > 0);
            Debug.Assert(output.length > 0);
            var result = new IntsRef(prefix.length + output.length);
            Array.Copy(prefix.ints, prefix.offset, result.ints, 0, prefix.length);
            Array.Copy(output.ints, output.offset, result.ints, prefix.length, output.length);
            result.length = prefix.length + output.length;
            return result;
        }

        public override IntsRef Common(IntsRef pair1, IntsRef pair2)
        {
            if (pair1 == null) throw new ArgumentNullException("pair1");
            if (pair2 == null) throw new ArgumentNullException("pair2");

            var pos1 = pair1.offset;
            var pos2 = pair2.offset;
            var stopAt1 = pos1 + Math.Min(pair1.length, pair2.length);
            while (pos1 < stopAt1)
            {
                if (pair1.ints[pos1] != pair2.ints[pos2])
                    break;
                pos1++;
                pos2++;
            }

            if (pos1 == pair1.offset) return NO_OUTPUT;
            if (pos1 == pair1.offset + pair1.length) return pair1;
            if (pos2 == pair2.offset + pair2.length) return pair2;
            return new IntsRef(pair1.ints, pair2.offset, pos1 - pair1.offset);
        }

        public override IntsRef GetNoOutput()
        {
            return NO_OUTPUT;
        }

        public override String OutputToString(IntsRef output)
        {
            return output.ToString();
        }

        public override IntsRef Read(DataInput dataInput)
        {
            var len = dataInput.ReadVInt();
            if (len == 0) return NO_OUTPUT;

            var output = new IntsRef(len);
            for (var idx = 0; idx < len; idx++)
                output.ints[idx] = dataInput.ReadVInt();

            output.length = len;
            return output;
        }

        public override IntsRef Subtract(IntsRef output, IntsRef inc)
        {
            if (output == null) throw new ArgumentNullException("output");
            if (inc == null) throw new ArgumentNullException("inc");

            if (inc == NO_OUTPUT) return output;
            if (inc.length == output.length) return NO_OUTPUT;
            
            // TODO: assert correct here?
            Debug.Assert(inc.length < output.length, "inc.length=" + inc.length + " vs output.length=" + output.length);
            Debug.Assert(inc.length > 0);
            return new IntsRef(output.ints, output.offset + inc.length, output.length - inc.length);
        }

        public override void Write(IntsRef prefix, DataOutput dataOutput)
        {
            if (prefix == null) throw new ArgumentNullException("prefix");
            dataOutput.WriteVInt(prefix.length);
            for (var idx = 0; idx < prefix.length; idx++)
                dataOutput.WriteVInt(prefix.ints[prefix.offset + idx]);
        }
    }
}
