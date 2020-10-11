using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigTicTacToe
{
    public class Move
    {
        public int row { get; private set; }
        public int col{ get; private set; } 
        public bool isXTurn { get; private set; }

        public Move(int row, int col, bool isXTurn) {
            this.row = row;
            this.col = col;
            this.isXTurn = isXTurn;
        }
    }
}
