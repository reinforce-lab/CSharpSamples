using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.ReinforceLab.SignalTrace
{
    public class SpiceTr0Formatter
    {
        /*
#H
SOURCE='HSPICE' VERSION='97.2 '  
ANALYSIS='TR' SWEEPVAR='TIME'       
SWEEPMODE='VAR_STEP' COMPLEXVALUES='NO' FORMAT='1 VOLTSorAMPS;EFLOAT'         
XBEGIN='  .00000E+00' XEND=' 1.00000E-06'                  
NODES='8'
#N 'V(frosc)' 'V(posc)' 'V(vcc)' 'V(vss)' 'I(vcc)' 'I(vss)'             
'I(vfrosc)' 'I(vposc)'  

#C    0.             8     0.          0.          0.          0. 0.          0.          0.          0.       

#C  2.5000000e-10    8     0.45000     0.45000   4.5000e-03    0.       -4.5000e-09    0.       -4.5000e-07 -4.5000e-07 

#C  5.0000000e-10    8     0.90000     0.90000   9.0000e-03    0. -9.0000e-09    0.       -9.0000e-07 -9.0000e-07 

#C  7.5000000e-10    8     1.35000     1.35000   1.3500e-02    0. -1.3500e-08    0.       -1.3500e-06 -1.3500e-06 

#C  1.0000000e-09    8     1.80000     1.80000   1.8000e-02    0.       -1.8000e-08    0.       -1.8000e-06 -1.8000e-06 

#;
         */

        #region Variables
        const string header_template = "#H\nSOURCE='HSPICE' VERSION='97.2 '\nANALYSIS='TR' SWEEPVAR='TIME'\nSWEEPMODE='VAR_STEP' COMPLEXVALUES='NO' FORMAT='1 VOLTSorAMPS;EFLOAT'\nXBEGIN='{0:e}' XEND='{1:e}'\nNODES='{2}'\n";
        #endregion


        #region Public methods
        public void Dump(System.IO.TextWriter writer, double timestep, ITracer[] recorders)
        {
            if (recorders.Length <= 0)
                return;

            int sample_length = int.MaxValue;
            foreach (var recorder in recorders)
            {
                sample_length = Math.Min(recorder.Signal.Count<double>(), sample_length);
            }
            // header
            writer.Write(String.Format(header_template, 
                0,
                timestep * sample_length,
                recorders.Length));
            writer.Write("#N");
            foreach (var recorder in recorders)
            {
                writer.Write(String.Format(" '{0}'", recorder.SignalName));
            }
            writer.Write("\n\n");
            // body
            var values = new List<Double[]>();
            foreach (var recorder in recorders)
            {
                values.Add(recorder.Signal.ToArray<Double>());                
            }

            for (int indent = 0; indent < sample_length; indent++)
            {
                writer.Write(String.Format("#C {0:E} {1}", indent * timestep, recorders.Length));
                foreach (var value in values)
                {
                    writer.Write(String.Format(" {0:e}", value[indent]));
                }
                writer.Write("\n\n");
            }
            // footer
            writer.Write("#;\n");
        }
        #endregion
    }
}
