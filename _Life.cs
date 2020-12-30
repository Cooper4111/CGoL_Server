using System;

using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

/*
>> TODO
- Field Width, Height -- get public, set private.
- Docstrings
- Divide cstream into:  1) Engine   2) cout (cout => coutFrame)
- Move all console output to ServerGUI.cs
- cells2kill, cells2born -- DO NOT check cells checked already (add 2 new hashsets)
- Normalize: inline => reference (to class Field? Or let it be inline? Think.)
- Null check everywhere if required. CHECK. EVERYWHERE.
- Try...Catch everywhere if required
- Raise exceptions on: 
    - field dimensions

>> TOTEST
- cells2kill, cells2born, cellMap => dictionary{hash:(x,y)}
- field.Width VS Width (is it internally this.Width, negating all potential perfomance boost?)
    - if(first): everywhere: this.Width => field.Width
*/

namespace LifeServer
{

    class _Life
    {
        private class Field
        {
            int Width, Heigth;
            byte[,] field;

            public Field(int Width, int Heigth)
            {
                this.Width  = Width;
                this.Heigth = Heigth;
                field = new byte[Width, Heigth];
                zeroInit();

            }
            void zeroInit()
            {
                for (int H = 0; H < Heigth; H++)
                {
                    for (int W = 0; W < Width; W++)
                    {
                        field[W, H] = 0;
                    }
                }
            }
            public byte this[int x, int y]
            {
                get
                {
                    x = (x + Width) % Width;
                    y = (y + Heigth) % Heigth;
                    return field[x, y];
                }
                set
                {
                    x = (x + Width) % Width;
                    y = (y + Heigth) % Heigth;
                    field[x, y] = value;
                }
            }
        }

        int Width;
        int Heigth;
        const int playerNum = 7; // clarify number of possible players, let it be 7 for now
        Field field;
        Random rnd;
        HashSet<int> cells2born;
        HashSet<int>[] cells2bornArr;
        HashSet<int>[] cells2dieArr;
        HashSet<int>[] cellMapArr;
        
        public _Life(int ArgWidth, int ArgHeigth)
        {
            Width         = ArgWidth;
            Heigth        = ArgHeigth;
            field         = new Field(Width, Heigth);
            cells2born    = new HashSet<int>(256);
            cells2dieArr  = new HashSet<int>[playerNum];
            cells2bornArr = new HashSet<int>[playerNum]; 
            cellMapArr    = new HashSet<int>[playerNum];

            for (int i = 0; i < playerNum; i++)
                cells2bornArr[i] = new HashSet<int>(256);
            for (int i = 0; i < playerNum; i++)
                cells2dieArr[i] = new HashSet<int>(256);
            for (int i = 0; i < playerNum; i++)
                cellMapArr[i] = new HashSet<int>(256);

            rnd = new Random(DateTimeOffset.Now.Second);
            DrawGlider(4, 4, "NW");
        }

        int TotalCells()
        {
            int totalCells = 0;
            int i = 0;
            for(int index = 0; index < playerNum; index++)
            {
                i++;
                totalCells += cellMapArr[index].Count;
            }
            return totalCells;
        }

        /// <summary> Consequently writes int[] chunks of following structure {[1] count, [2] color, [3...n] coordinate hashes} to dest </summary>

        public void GetCellMap(ref int[] dest) // -> wrapper (?)
        {
            int offset = 0;
            int totalCells = TotalCells();
            dest = new int[totalCells + playerNum*2];
            for (byte playerIndex = 0; playerIndex < playerNum; playerIndex++)
            {
                offset += 2;
                cellMapArr[playerIndex].CopyTo(dest, offset);
                dest[offset-2] = cellMapArr[playerIndex].Count;
                dest[offset-1] = Accounts.GetIntColor(Accounts.ID2username[playerIndex + 1]);
                offset += dest[offset - 2];
            }
        }
        int Crd2hash(int x, int y)
        {
            x = (x + Width) % Width;
            y = (y + Heigth) % Heigth;
            return x + y * Width;
        }
        int[] Hash2crd(int h)
        {
            int x = h % this.Width;
            return new int[2] { x, (h - x) / this.Width };
        }
        void DelCell(int hash, byte playerIndex)
        {
            cellMapArr[playerIndex].Remove(hash);
            int[] crd = Hash2crd(hash);
            field[crd[0], crd[1]] = 0;
        }
        void AddCell(int hash, byte playerID)
        {
            cellMapArr[playerID-1].Add(hash);
            int[] crd = Hash2crd(hash);
            field[crd[0], crd[1]] = playerID;

        }

        /// <returns>ID (NOT index!) of player, whose cell to born. ID = 0 means no need to born.</returns>
        byte MustBorn(int x, int y)
        {
            byte[] N = new byte[8];
            byte count = 0;
            //
            // в отедльную функцию (?) foreach (arr[8]) -- массив с дельтами координат
            //
            if (field[x - 1, y - 1] > 0) { N[0] = field[x - 1, y - 1]; count++; }
            if (field[x - 1, y] > 0)     { N[1] = field[x - 1, y];     count++; }
            if (field[x - 1, y + 1] > 0) { N[2] = field[x - 1, y + 1]; count++; }
            if (field[x, y - 1] > 0)     { N[3] = field[x, y - 1];     count++; }
            if (field[x, y + 1] > 0)     { N[4] = field[x, y + 1];     count++; }
            if (field[x + 1, y - 1] > 0) { N[5] = field[x + 1, y - 1]; count++; }
            if (field[x + 1, y] > 0)     { N[6] = field[x + 1, y];     count++; }
            if (field[x + 1, y + 1] > 0) { N[7] = field[x + 1, y + 1]; count++; }
            if (count == 3)
            {
                byte[] tri = new byte[3];
                byte found = 0;
                byte index = 0;
                while (found != 3)
                {
                    if (N[index] > 0)
                    {
                        tri[found] = N[index];
                        found++;
                    }
                    index++;
                }
                if (tri[0] == tri[1])
                    return tri[0];
                if (tri[1] == tri[2])
                    return tri[1];
                if (tri[2] == tri[0])
                    return tri[2];
                return tri[rnd.Next(3)]; // in case if there are three cells of different color around
            }
            else
            {
                return 0;
            }
        }
        bool MustDie(int x, int y)
        {
            byte N = 0;
            if (this.field[x - 1, y - 1] > 0)
                N++;
            if (this.field[x - 1, y] > 0)
                N++;
            if (this.field[x - 1, y + 1] > 0)
                N++;
            if (this.field[x, y - 1] > 0)
                N++;
            if (this.field[x, y + 1] > 0)
                N++;
            if (this.field[x + 1, y - 1] > 0)
                N++;
            if (this.field[x + 1, y] > 0)
                N++;
            if (this.field[x + 1, y + 1] > 0)
                N++;
            if (N == 3 || N == 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        void CheckBorn(int _x, int _y)
        {
            byte cellPlayerID;
            int hash, x, y;

            x = _x - 1;
            y = _y - 1;
            hash = Crd2hash(x, y);
            // в отдельную функцию:
            if (field[x, y] == 0 && !cells2born.Contains(hash))
            {
                cellPlayerID = MustBorn(x, y);
                if (cellPlayerID > 0)
                {
                    cells2bornArr[cellPlayerID-1].Add(hash);
                    cells2born.Add(hash);
                }
            }
            x = _x;
            y = _y - 1;
            hash = Crd2hash(x, y);
            if (field[x, y] == 0 && !cells2born.Contains(hash))
            {
                cellPlayerID = MustBorn(x, y);
                if (cellPlayerID > 0)
                {
                    cells2bornArr[cellPlayerID - 1].Add(hash);
                    cells2born.Add(hash);
                }
            }
            x = _x + 1;
            y = _y - 1;
            hash = Crd2hash(x, y);
            if (field[x, y] == 0 && !cells2born.Contains(hash))
            {
                cellPlayerID = MustBorn(x, y);
                if (cellPlayerID > 0)
                {
                    cells2bornArr[cellPlayerID - 1].Add(hash);
                    cells2born.Add(hash);
                }
            }
            x = _x + 1;
            y = _y;
            hash = Crd2hash(x, y);
            if (field[x, y] == 0 && !cells2born.Contains(hash))
            {
                cellPlayerID = MustBorn(x, y);
                if (cellPlayerID > 0)
                {
                    cells2bornArr[cellPlayerID - 1].Add(hash);
                    cells2born.Add(hash);
                }
            }
            x = _x + 1;
            y = _y + 1;
            hash = Crd2hash(x, y);
            if (field[x, y] == 0 && !cells2born.Contains(hash))
            {
                cellPlayerID = MustBorn(x, y);
                if (cellPlayerID > 0)
                {
                    cells2bornArr[cellPlayerID - 1].Add(hash);
                    cells2born.Add(hash);
                }
            }
            x = _x;
            y = _y + 1;
            hash = Crd2hash(x, y);
            if (field[x, y] == 0 && !cells2born.Contains(hash))
            {
                cellPlayerID = MustBorn(x, y);
                if (cellPlayerID > 0)
                {
                    cells2bornArr[cellPlayerID - 1].Add(hash);
                    cells2born.Add(hash);
                }
            }
            x = _x - 1;
            y = _y + 1;
            hash = Crd2hash(x, y);
            if (field[x, y] == 0 && !cells2born.Contains(hash))
            {
                cellPlayerID = MustBorn(x, y);
                if (cellPlayerID > 0)
                {
                    cells2bornArr[cellPlayerID - 1].Add(hash);
                    cells2born.Add(hash);
                }
            }
            x = _x - 1;
            y = _y;
            hash = Crd2hash(x, y);
            if (field[x, y] == 0 && !cells2born.Contains(hash))
            {
                cellPlayerID = MustBorn(x, y);
                if (cellPlayerID > 0)
                {
                    cells2bornArr[cellPlayerID - 1].Add(hash);
                    cells2born.Add(hash);
                }
            }
        }
        void CheckDie(int x, int y, int hash, byte playerIndex)
        {
            if (field[x, y] != 0 && MustDie(x, y))
                cells2dieArr[playerIndex].Add(hash);
        }
        void KillCells()
        {
            for (byte playerIndex = 0; playerIndex < cells2dieArr.Length; playerIndex++)
            {
                foreach (int hash in cells2dieArr[playerIndex])
                {
                    DelCell(hash, playerIndex);
                }
            }
        }
        void BornCells()
        {
            for (byte playerIndex = 0; playerIndex < cells2bornArr.Length; playerIndex++)
            {
                foreach (int hash in cells2bornArr[playerIndex])
                {
                    AddCell(hash, (byte)(playerIndex + 1));
                }
            }
        }
        public void IterateOnce()
        {
            cells2born = new HashSet<int>(256);
            for (int i = 0; i < playerNum; i++)
                cells2bornArr[i] = new HashSet<int>(256);
            for (int i = 0; i < playerNum; i++)
                cells2dieArr[i]  = new HashSet<int>(256);
            int[] crd;
            for (byte playerIndex = 0; playerIndex < cellMapArr.Length; playerIndex++)
            {
                foreach (int hash in cellMapArr[playerIndex])
                {
                    crd = Hash2crd(hash);
                    CheckBorn(crd[0], crd[1]);
                    CheckDie(crd[0], crd[1], hash, playerIndex); // распараллелить
                }
            }
            KillCells();
            BornCells();
        }
        public void AddStructure(int[] hashes, byte playerID)
        {
            foreach (int hash in hashes)
            {
                AddCell(hash, playerID);
            }
        }


        // ############### GLIDER FOR TESTING #######################
        void AddCell(int x, int y, byte playerID)
        {
            cellMapArr[playerID-1].Add(Crd2hash(x, y));
            field[x, y] = playerID;
        }
        void DrawGlider(int x = 1, int y = 1, string option = "SE")
        {
            if (option == "SE")
            {
                AddCell(x, y + 2, 1);
                AddCell(x + 1, y + 2, 1);
                AddCell(x + 2, y + 2, 1);
                AddCell(x + 2, y + 1, 1);
                AddCell(x + 1, y, 1);
            }
            if (option == "NW")
            {
                AddCell(x, y, 1);
                AddCell(x + 1, y, 1);
                AddCell(x + 2, y, 1);
                AddCell(x, y + 1, 1);
                AddCell(x + 1, y + 2, 1);
            }
        }
    }
}

/* DevNotes:

>>NOTE 001:
Построчная проверка всех соседей, без циклов --
это на самом деле действительно единственный рациональный подход.
Добавление кустов из циклов займёт столько же места,
только ещё и снизит производительность. А это боттлнек.

>>NOTE 002:
В отличие от взятия остатка (a%b), модуль числа -- дорогая операция (взятие остатка != кольцо вычетов по модулю).
Поэтому пришлось сделать костыль с сокращением доступного диапазона до abs(int)/2
с допущением, что смещение по индексу не будет больше половины высоты/ширины поля
(но фактически, сейчас оно не может быть больше 1). Подобный подход позвляет остаться в рамках
использования операции взятия остатка.
Подробнее в документации (которая пока что набросана от руки в тетради) 
*/