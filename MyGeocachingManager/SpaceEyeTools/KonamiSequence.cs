using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SpaceEyeTools
{
    /// <summary>
    /// A class to detected a Konami sequence played with keyoard
    /// Reminder: up, up, down, down, left, right, left, right, B, A
    /// </summary>
    public class KonamiSequence
    {

        List<Keys> Keys = new List<Keys>{System.Windows.Forms.Keys.Up, System.Windows.Forms.Keys.Up, 
                                       System.Windows.Forms.Keys.Down, System.Windows.Forms.Keys.Down, 
                                       System.Windows.Forms.Keys.Left, System.Windows.Forms.Keys.Right, 
                                       System.Windows.Forms.Keys.Left, System.Windows.Forms.Keys.Right, 
                                       System.Windows.Forms.Keys.B, System.Windows.Forms.Keys.A};
        private int mPosition = -1;

        /// <summary>
        /// Get / Set position in sequence
        /// </summary>
        public int Position
        {
            get { return mPosition; }
            private set { mPosition = value; }
        }

        /// <summary>
        /// Check completion of the sequence
        /// </summary>
        /// <param name="key">last key pressed</param>
        /// <returns>true is sequence is completed</returns>
        public bool IsCompletedBy(Keys key)
        {

            if (Keys[Position + 1] == key)
            {
                // move to next
                Position++;
            }
            else if (Position == 1 && key == System.Windows.Forms.Keys.Up)
            {
                // stay where we are
            }
            else if (Keys[0] == key)
            {
                // restart at 1st
                Position = 0;
            }
            else
            {
                // no match in sequence
                Position = -1;
            }

            if (Position == Keys.Count - 1)
            {
                Position = -1;
                return true;
            }

            return false;
        }
    }
}