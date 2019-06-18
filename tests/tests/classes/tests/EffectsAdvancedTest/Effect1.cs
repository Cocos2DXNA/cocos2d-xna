using Cocos2D;

namespace tests
{
    public class Effect1 : EffectAdvanceTextLayer
    {
        public override void OnEnter()
        {
            base.OnEnter();

            CCNode target = GetChildByTag(EffectAdvanceScene.kTagBackground);

            // To reuse a grid the grid size and the grid type must be the same.
            // in this case:
            //     Lens3D is Grid3D and it's size is (15,10)
            //     Waves3D is Grid3D and it's size is (15,10)

            CCSize size = CCDirector.SharedDirector.WinSize;
            CCActionInterval lens = new CCLens3D(0.0f, new CCGridSize(15, 10), new CCPoint(size.Width / 4, size.Height / 4), 240);
            CCActionInterval waves = new CCWaves3D(10, new CCGridSize(15, 10), 18, 15);


            CCFiniteTimeAction reuse = new CCReuseGrid(1);
            CCActionInterval delay = new CCDelayTime (8);

            CCActionInterval orbit = new CCOrbitCamera(5, 1, 2, 0, 180, 0, -90);
            CCFiniteTimeAction orbit_back = orbit.Reverse();

            target.RunAction(new CCRepeatForever ((new CCSequence(orbit, orbit_back))));
            target.RunAction(new CCSequence(lens, delay, reuse, waves));
        }

        public override string title()
        {
            return "Lens + Waves3d and OrbitCamera";
        }
    }
}