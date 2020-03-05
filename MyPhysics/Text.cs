using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace AlienScribble {
    class Text {
        public QuadBatch quad;
        Input inp;
        Rectangle pixel;
        int swidth, sheight;
        Vector2 center;
        int last_letter_x, last_letter_y;
        int letter_width, letter_height;
        bool old_dbord; Color old_color; float old_size;   // store old states: dark_border, color, Size
        public bool dark_border = true;
        public Color color;
        public Color reg_color = new Color(15, 255, 0, 255), dark_color = new Color(0, 0, 0, 90);
        public bool is_over_text = false;
        public float Size {
            get { return quad.Default_Font_Size; }
            set { quad.Default_Font_Size = value; }
        }


        // C O N S T R U C T O R        
        public Text(QuadBatch Quad, int ScreenWidth, int ScreenHeight, Input Inp, Rectangle? PixelRect)
        {
            pixel = new Rectangle(0, 0, 1, 1); if (PixelRect.HasValue) pixel = PixelRect.Value;
            swidth = ScreenWidth; sheight = ScreenHeight;
            center = new Vector2(swidth / 2, sheight / 2);
            inp = Inp;
            quad = Quad;
            color = reg_color;
            Vector2 font_size = new Vector2(quad.Average_Width, quad.text_v_space);
            last_letter_x = last_letter_y = 0; letter_width = (int)font_size.X; letter_height = (int)font_size.Y;
        }


        // F I L L  R E C T
        public void FillRect(int x, int y, int w, int h, Color col)
        {
            quad.FillRect(pixel, new Rectangle(x, y, w, h), col);
        }
        // S A V E  S T A T E S 
        public void SaveStates() { old_dbord = dark_border; old_color = color; old_size = Size; }
        // R E S T O R E  S T A T E S 
        public void RetoreStates() { dark_border = old_dbord; color = old_color; Size = old_size; }


        // D R A W
        public void Draw(int x, int y, String s)
        {
            if (dark_border) {
                Vector2 sz = quad.MeasureStringFast(s); quad.DrawDest(pixel, new Rectangle(x, y, (int)(sz.X * 1.01f), (int)(sz.Y * 0.8f)), dark_color);
            }
            quad.DrawStringFast(s, new Vector2((float)x, (float)y), color);
        }
        // D R A W
        public void Draw(int x, int y, String s, int x2, string s2, Color c1, Color c2)
        {
            quad.DrawStringFast(s, new Vector2((float)x, (float)y), c1);
            quad.DrawStringFast(s2, new Vector2((float)x2, (float)y), c2);
        }


        // D R A W  C E N T E R E D
        public void DrawCentered(int x, int y, String s)
        {
            Vector2 cent = center;
            Vector2 offset = quad.MeasureStringFast(s);
            last_letter_x = (int)(center.X + offset.X / 2 + x - letter_width / 2); last_letter_y = (int)(center.Y + offset.Y / 2 + y - letter_height); //used if needed for cursor (elsewhere)                       
            cent.X = center.X - offset.X / 2 + x; cent.Y = center.Y - offset.Y / 2 + y;
            if (dark_border) {
                quad.DrawDest(pixel, new Rectangle((int)cent.X, (int)cent.Y, (int)offset.X + letter_width / 2, (int)offset.Y), dark_color);
            }
            quad.DrawStringFast(s, cent, color);
        }



        // D R A W  C L I C K A B L E
        /// Draw clickable text that turns yellow when hovered apon:
        public bool DrawClickable(int x, int y, String s, int mosX, int mosY, bool mouseClick)
        {
            Vector2 sz = quad.MeasureStringFast(s);
            if (dark_border) {
                quad.DrawDest(pixel, new Rectangle(x, y, (int)(sz.X * 1.01f), (int)(sz.Y * 0.8f)), dark_color);
            }
            is_over_text = false;
            if ((mosX > x) && (mosY > y) && (mosX < (x + sz.X)) && (mosY < (y + sz.Y * 0.7f))) {
                quad.DrawStringFast(s, new Vector2((float)x, (float)y), Color.Yellow); is_over_text = true;
                if (mouseClick) { inp.leftClick = false; return true; }
            }
            else quad.DrawStringFast(s, new Vector2((float)x, (float)y), color);
            return false;
        }



        // D R A W  C L I C K A B L E  C E N T E R E D
        public bool DrawClickableCentered(int x, int y, String s, int mosX, int mosY, bool mouseClick)
        {
            Vector2 sz = quad.MeasureStringFast(s), cent = center;
            cent.X = center.X - sz.X / 2 + x; cent.Y = center.Y - sz.Y / 2 + y;
            if (dark_border) {
                quad.DrawDest(pixel, new Rectangle((int)cent.X, (int)cent.Y, (int)sz.X + letter_width / 2, (int)sz.Y), dark_color);
            }
            is_over_text = false;
            if (mosX > cent.X && mosY > cent.Y && mosX < cent.X + sz.X * 1.1f && mosY < cent.Y + sz.Y * 0.85f) {
                quad.DrawStringFast(s, cent, Color.Yellow); is_over_text = true;
                if (mouseClick) { inp.leftClick = false; return true; }
            }
            else quad.DrawStringFast(s, cent, color);
            return false;
        }



        //-------------------------------------------
        // I N P U T  (polls for string input and returns true when the user is done entering input)
        public bool getting_text_input = false;
        private float timer; //cursor timing   
        private KeyboardState currentKeyboardState, lastKeyboardState;
        //used for keyboard input thing: 
        private Keys[] keysToCheck = new Keys[] { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J, Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z, Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.Space, Keys.Back, Keys.OemPeriod, Keys.Decimal, Keys.OemMinus, Keys.Divide, Keys.OemQuestion };
        private int[] keyTime = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        // position, current input, size restrictions(like for file saving)
        public bool Input(int x, int y, ref String input_str, int min_length, int max_length, GameTime gameTime, bool first_letter_restrictions, bool center_input)
        {
            bool shift = false;
            getting_text_input = true;
            timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds; if (timer > 200f) timer = 0;
            currentKeyboardState = Keyboard.GetState(); if (currentKeyboardState.IsKeyDown(Keys.Escape)) { getting_text_input = false; inp.flush(); return false; }
            if (currentKeyboardState.IsKeyDown(Keys.Enter) && lastKeyboardState.IsKeyUp(Keys.Enter)) if (input_str.Length >= min_length) { getting_text_input = false; inp.flush(); return true; }
            if (currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift)) shift = true;
            if (input_str.Length > 0)
                if (center_input) { DrawCentered(x, y, input_str); if (timer < 100f) Draw(last_letter_x, last_letter_y, "_"); } else Draw(x, y, input_str);
            else { DrawCentered(x, y, " "); if (timer < 100f) Draw(last_letter_x, last_letter_y, "_"); }
            int a = -1;
            foreach (Keys key in keysToCheck) {
                a++;
                if (lastKeyboardState.IsKeyDown(key) && currentKeyboardState.IsKeyDown(key)) {
                    keyTime[a]++; //for held key repeats and delays
                }
                else keyTime[a] = 0;
                bool do_it = false;
                if (keyTime[a] > 15) { do_it = true; keyTime[a] = 7; }
                if (lastKeyboardState.IsKeyUp(key) && currentKeyboardState.IsKeyDown(key)) do_it = true;
                if (do_it) {
                    String newChar = "";
                    if (input_str.Length >= max_length && key != Keys.Back) continue; //don't do anything if we reached size limit (unless is Back key)                    
                    switch (key) {
                        case Keys.A: newChar += "a"; break;
                        case Keys.B: newChar += "b"; break;
                        case Keys.C: newChar += "c"; break;
                        case Keys.D: newChar += "d"; break;
                        case Keys.E: newChar += "e"; break;
                        case Keys.F: newChar += "f"; break;
                        case Keys.G: newChar += "g"; break;
                        case Keys.H: newChar += "h"; break;
                        case Keys.I: newChar += "i"; break;
                        case Keys.J: newChar += "j"; break;
                        case Keys.K: newChar += "k"; break;
                        case Keys.L: newChar += "l"; break;
                        case Keys.M: newChar += "m"; break;
                        case Keys.N: newChar += "n"; break;
                        case Keys.O: newChar += "o"; break;
                        case Keys.P: newChar += "p"; break;
                        case Keys.Q: newChar += "q"; break;
                        case Keys.R: newChar += "r"; break;
                        case Keys.S: newChar += "s"; break;
                        case Keys.T: newChar += "t"; break;
                        case Keys.U: newChar += "u"; break;
                        case Keys.V: newChar += "v"; break;
                        case Keys.W: newChar += "w"; break;
                        case Keys.X: newChar += "x"; break;
                        case Keys.Y: newChar += "y"; break;
                        case Keys.Z: newChar += "z"; break;
                        case Keys.D0: newChar += "0"; break;
                        case Keys.D1: newChar += "1"; break;
                        case Keys.D2: newChar += "2"; break;
                        case Keys.D3: newChar += "3"; break;
                        case Keys.D4: newChar += "4"; break;
                        case Keys.D5: newChar += "5"; break;
                        case Keys.D6: newChar += "6"; break;
                        case Keys.D7: newChar += "7"; break;
                        case Keys.D8: newChar += "8"; break;
                        case Keys.D9: newChar += "9"; break;
                        case Keys.Space: if (!((first_letter_restrictions) && (input_str.Length < 1))) newChar += " "; break;
                        case Keys.OemPeriod: if (!((first_letter_restrictions) && (input_str.Length < 1))) newChar += "."; break;
                        case Keys.Decimal: if (!((first_letter_restrictions) && (input_str.Length < 1))) newChar += "."; break;
                        case Keys.OemMinus: if (!((first_letter_restrictions) && (input_str.Length < 1))) { if (!shift) newChar += "-"; else newChar += "_"; } break;
                        case Keys.Divide: if (!((first_letter_restrictions) && (input_str.Length < 1))) newChar += "/"; break;
                        case Keys.OemQuestion: if (!((first_letter_restrictions) && (input_str.Length < 1))) { if (!shift) newChar += "/"; else newChar += "?"; } break;
                        case Keys.Back: if (input_str.Length != 0) input_str = input_str.Remove(input_str.Length - 1, 1); continue;
                    }
                    if (shift) newChar = newChar.ToUpper();
                    input_str += newChar;
                    break;
                }
            }
            lastKeyboardState = currentKeyboardState;
            return false;
        }//Input      


        public bool reset_keypress_tester = false;    //using to make sure button isn't held before entering menu - needs a release before will accept input (make sure this is false to prevent re-resets)
        public KeyboardState last_keypress_states;    //using to make sure button isn't held before entering menu - needs a release before will accept input        

        public void reset_keypress_test()
        {
            last_keypress_states = Keyboard.GetState();
            reset_keypress_tester = true;
        }


        //-------------------------------------------
        // A S K  Y E S  N O 
        // Prompt for a yes-no keypress(Y/N)   [ set -1, -1 to center prompt ]
        public bool ask_YesNo(int x, int y, String s, ref int yes)
        {
            dark_border = true;
            if ((x < 0) || (y < 0)) DrawCentered(0, -12, s); else Draw(x, y, s);
            dark_border = false;
            currentKeyboardState = Keyboard.GetState();
            if (currentKeyboardState.IsKeyDown(Keys.N)) { yes = 0; return true; }
            //ok so test to see if Y is being held before entering this poll and only accept a response if the key is first released at least once
            if (reset_keypress_tester) {
                if (last_keypress_states.IsKeyDown(Keys.Y)) return false; //not acceptable! Key was being help (possibly accidental)
                else reset_keypress_tester = false;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Y)) { yes = 1; reset_keypress_tester = false; return true; }
            last_keypress_states = currentKeyboardState;
            return false; // not done polling
        }//ask_YesNo
    }

    // G E T  L A S T  (helpful for seeing if there's a file extension already typed in by the user 
    // (checks a substring of whatever length at the end of the string)
    public static class StringExtension {
        public static string GetLast(this String s, int tail_length)
        {
            if (tail_length >= s.Length) return s;
            return s.Substring(s.Length - tail_length);
        }
    }
}
