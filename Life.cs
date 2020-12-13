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
    class Field{
        int Width, Heigth;
        byte[,] field;

        public Field(int Width, int Heigth){
            this.Width  = Width;
            this.Heigth = Heigth;
            field       = new byte[Width,Heigth];
            zeroInit();
            
        }
        void zeroInit(){
            for(int H = 0; H < Heigth; H++){
                for(int W = 0; W < Width; W++){
                    field[W,H] = 0;
                }
            }            
        }
        public byte this[int x, int y]
        {
            get{
                x = (x+Width) % Width;
                y = (y+Heigth) % Heigth;
                return field[x,y];
            } 
            set{
                x = (x+Width) % Width;
                y = (y+Heigth) % Heigth;
                field[x,y] = value;
            }
        }
    }

    class Life
    {   
        int          Width;
        int          Heigth;
        Field        field;
        HashSet<int> cellMap;
        HashSet<int> cells2kill;
        HashSet<int> cells2born;

        public Life(int Width, int Heigth)
        {
            this.Width      = Width;
            this.Heigth     = Heigth;
            this.field      = new Field(Width,Heigth);
            this.cellMap    = new HashSet<int>(256);
            this.cells2kill = new HashSet<int>(256);
            this.cells2born = new HashSet<int>(256);
            drawGlider(6,2, "NW");
        }
        
        public void getCellMap(ref int[] dest){

            dest = new int[cellMap.Count];
            cellMap.CopyTo(dest);

        }
        /// <summary>
        /// DEPRECATED, for testing only
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="option"></param>
        void drawGlider(int x = 1, int y = 1, string option = "SE"){
            if(option == "SE"){
                addCell(x,y+2);
                addCell(x+1,y+2);
                addCell(x+2,y+2);
                addCell(x+2,y+1);
                addCell(x+1,y);
            }if(option == "NW"){
                addCell(x,y);
                addCell(x+1,y);
                addCell(x+2,y);
                addCell(x,y+1);
                addCell(x+1,y+2);
            }
        }
        /// <summary>
        /// DEPRECATED, for testing only
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void drawStick(int x = 1, int y = 1){ // Vertical
            addCell(x+1,y);
            addCell(x+1,y+1);
            addCell(x+1,y+2);
        }

        int crd2hash(int x, int y){
            x = (x+Width) % Width;
            y = (y+Heigth) % Heigth;
            return x + y * Width;
        }
        int[] hash2crd(int h){
            int x = h % this.Width;
            return new int[2]{x, (h-x)/this.Width};
        }
        /// <summary>
        /// DEPRECATED, for testing only
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void delCell(int x, int y)
        {
            this.cells2kill.Remove(crd2hash(x,y));
            this.cellMap.Remove(crd2hash(x,y));
            this.field[x,y] = 0;
        }
        void delCell(int hash){
            this.cells2kill.Remove(hash);
            this.cellMap.Remove(hash);
            int[] crd = hash2crd(hash);
            this.field[crd[0],crd[1]] = 0;
        }
        /// <summary>
        /// DEPRECATED, for testing only
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void addCell(int x, int y)
        {
            this.cells2born.Remove(crd2hash(x,y));
            this.cellMap.Add(crd2hash(x,y));
            this.field[x,y] = 1;
        }
        void addCell(int hash){
            this.cells2born.Remove(hash);
            this.cellMap.Add(hash);
            int[] crd = hash2crd(hash);
            this.field[crd[0],crd[1]] = 1;

        }

        bool mustBorn(int x, int y){
            if(field[x,y] > 0 || cells2born.Contains(crd2hash(x,y))){
                return false;
            }
            byte N = 0;
            if(this.field[x-1,y-1] > 0) { N++; }
            if(this.field[x-1,y  ] > 0) { N++; }
            if(this.field[x-1,y+1] > 0) { N++; }
            if(this.field[x,  y-1] > 0) { N++; }
            if(this.field[x,  y+1] > 0) { N++; }
            if(this.field[x+1,y-1] > 0) { N++; }
            if(this.field[x+1,y  ] > 0) { N++; }
            if(this.field[x+1,y+1] > 0) { N++; }
            if(N == 3){
                return true;
            }else{
                return false;
            }
        }
        bool mustDie(int x, int y){
            if(this.field[x,y] == 0){
                return false;
            }
            byte N = 0;
            if(this.field[x-1,y-1] > 0) {N++;}
            if(this.field[x-1,y]   > 0) {N++;}
            if(this.field[x-1,y+1] > 0) {N++;}
            if(this.field[x,y-1]   > 0) {N++;}
            if(this.field[x,y+1]   > 0) {N++;}
            if(this.field[x+1,y-1] > 0) {N++;}
            if(this.field[x+1,y]   > 0) {N++;}
            if(this.field[x+1,y+1] > 0) {N++;}
            if(N == 3 || N == 2){
                return false;
            }else{
                return true;
            }
        }
    
        void checkBorn(int x, int y){
            if(mustBorn(x-1, y+1)){ cells2born.Add(crd2hash(x-1,y+1)); }
            if(mustBorn(x-1, y-1)){ cells2born.Add(crd2hash(x-1,y-1)); }
            if(mustBorn(x-1, y))  { cells2born.Add(crd2hash(x-1,y));   }
            if(mustBorn(x+1, y+1)){ cells2born.Add(crd2hash(x+1,y+1)); }
            if(mustBorn(x+1, y-1)){ cells2born.Add(crd2hash(x+1,y-1)); }
            if(mustBorn(x+1, y))  { cells2born.Add(crd2hash(x+1,y));   }
            if(mustBorn(x,   y+1)){ cells2born.Add(crd2hash(x,y+1));   }
            if(mustBorn(x,   y-1)){ cells2born.Add(crd2hash(x,y-1));   }
        }
        void checkDie(int x, int y){
            if(mustDie(x, y)){ cells2kill.Add(crd2hash(x,y)); }
        }

        void killCells(){
            foreach (int foo in this.cells2kill){
                delCell(foo);
            }
        }
        void bornCells(){
            foreach (int foo in this.cells2born){
                addCell(foo);
            }
        }
    
        public void iterateOnce(){
            int[] crd;
            foreach (int foo in this.cellMap){
                crd = hash2crd(foo);
                checkBorn(crd[0], crd[1]);
                checkDie(crd[0], crd[1]);
            }
            killCells();
            bornCells();
        }

        public void addStructure(int[] hashes){
            foreach(int hash in hashes){
                addCell(hash);
            }
        }
// ############### OUTPUT METHODS #######################
// ###############   DEPRECATED   #######################

        public void cls(){
            for(int i = 0; i < 50; i++){
                Console.WriteLine("\r");
            }
        }

        void cout(){
            Console.WriteLine("\r");
            for (int H = 0; H < Heigth; H++) {
                for (int W = 0; W < Width; W++) {
                    Console.Write(field[W, H] > 0 ? '#':'.');
                    if (W == Width - 1) Console.WriteLine("\r");
                }
            }
            Console.SetCursorPosition(0, Console.WindowTop);            
        }

        public void cstream(int iters = 10, int speed = 200){
            cls();
            for(int i = 0; i < iters; i++){
                cout();
                iterateOnce();
                System.Threading.Thread.Sleep(speed);
            }
        }
    }
}

/* DevNotes:

>>NOTE 000:
Движок реализован через 2D byte массив и разреженные массивы
(хеш-сеты от координат для клеток на поле, клеток для удаления, клеток для рождения)
Можно было обойтись без массива, используя только хешсет и наличие в нём
элемента за Alive, а отсутствие -- за Dead. Но в перспективе одного бита недостаточно
т.к. планируется сделать разные "виды" клеток (например, для каждого игрока свой цвет)
Поэтому, в целях будущего расширения сразу используется byte-массив

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