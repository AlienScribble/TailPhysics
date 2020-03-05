using AlienScribble;
using Microsoft.Xna.Framework;

namespace MyPhysics {
    public class TailBone
    {
        public  Vector2 pos;
        private Vector2 vel;
        private float target_length;  // how long this bone wants to be        

        // CONSTRUCT
        public TailBone(float length = 30f) {
            target_length = length;              // bone will try to stay this length
        }

        // U P D A T E ---------------------------------------------------------------
        public void Update(Vector2 ThingToFollow, float x_bias, float horizontal_bias) // use horizontal bias to control how-much horizontal each part of the tail is
        {            
            Vector2 toward    = (ThingToFollow - pos);             // vector that points from current bone-tip location toward object it wants to follow
            float   distance  = toward.Length();                   // distance from thing it follows                        
                       
            pos = (pos*0.3f + (ThingToFollow - toward.Normal() * target_length)*0.7f);  // put position at the right distance along vector that points to target
            if (pos.Y - ThingToFollow.Y < target_length - horizontal_bias)      vel.Y += 0.14f; // add some gravity if tail isn't pointing down enough
            else if (pos.Y - ThingToFollow.Y > target_length - horizontal_bias) vel.Y -= 0.14f; // make it bounce if it goes too far
            if (x_bias < 0) vel.X += 0.06f;                                                     // acts like wind putting the tail behind the player
            if (x_bias > 0) vel.X -= 0.06f;
            vel *= 0.94f;      // reduces velocity over time (94% of previous vel for each frame)
            pos += vel;        // apply gravity (or bounce) and horizontal direction bias
        }
    }
}
