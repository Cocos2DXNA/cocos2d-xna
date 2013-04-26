using cocos2d;

namespace tests
{
    public class FlipX3DDemo : CCFlipX3D
    {
        public static CCActionInterval actionWithDuration(float t)
        {
            CCFlipX3D flipx = new CCFlipX3D(t);
            CCFiniteTimeAction flipx_back = flipx.Reverse();
            CCDelayTime delay = new CCDelayTime (2);

            return (CCSequence.FromActions(flipx, delay, flipx_back));
        }
    }
}