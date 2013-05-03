using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using cocos2d;
using CocosDenshion;
using System.Diagnostics;

namespace tests 
{
    public class CocosDenshionTest : CCLayer
    {
        string EFFECT_FILE = "Sounds/effect1";
        string MUSIC_FILE = "Sounds/background";
        int LINE_SPACE = 40;

        CCMenu m_pItmeMenu;
	    CCPoint m_tBeginPos;
	    int m_nTestCount;
	    int m_nSoundId;

        public CocosDenshionTest()
        {
            m_pItmeMenu = null;
            m_tBeginPos = new CCPoint(0,0);
            m_nSoundId = 0;

	        string[] testItems = {
		        "play background music",
		        "stop background music",
		        "pause background music",
		        "resume background music",
		        "rewind background music",
		        "is background music playing",
		        "play effect",
                "play effect repeatly",
		        "stop effect",
		        "unload effect",
		        "add background music volume",
		        "sub background music volume",
		        "add effects volume",
		        "sub effects volume"
	        };

	        // add menu items for tests
	        m_pItmeMenu = new CCMenu(null);
	        CCSize s = CCDirector.SharedDirector.WinSize;
	        m_nTestCount = testItems.Count<string>();

	        for (int i = 0; i < m_nTestCount; ++i)
	        {
                CCLabelTTF label = new CCLabelTTF(testItems[i], "arial", 24);
                CCMenuItemLabel pMenuItem = new CCMenuItemLabel(label, new SEL_MenuHandler(menuCallback));
		
		        m_pItmeMenu.AddChild(pMenuItem, i + 10000);
		        pMenuItem.Position = new CCPoint( s.Width / 2, (s.Height - (i + 1) * LINE_SPACE) );
	        }

	        m_pItmeMenu.ContentSize = new CCSize(s.Width, (m_nTestCount + 1) * LINE_SPACE);
	        m_pItmeMenu.Position = new CCPoint(0,0);
	        AddChild(m_pItmeMenu);

	        this.TouchEnabled = true;

	        // preload background music and effect
	        SimpleAudioEngine.SharedEngine.PreloadBackgroundMusic(CCFileUtils.FullPathFromRelativePath(MUSIC_FILE));
	        SimpleAudioEngine.SharedEngine.PreloadEffect(CCFileUtils.FullPathFromRelativePath(EFFECT_FILE));
    
            // set default volume
            SimpleAudioEngine.SharedEngine.EffectsVolume = 0.5f;
            SimpleAudioEngine.SharedEngine.BackgroundMusicVolume = 0.5f;
        }

        ~CocosDenshionTest()
        {
        }

        public override void OnExit()
        {
	        base.OnExit();

	        SimpleAudioEngine.SharedEngine.End();
        }

        public void menuCallback(object pSender)
        {
	        // get the userdata, it's the index of the menu item clicked
	        CCMenuItem pMenuItem = (CCMenuItem)(pSender);
	        int nIdx = pMenuItem.ZOrder - 10000;

	        switch(nIdx)
	        {
	        // play background music
	        case 0:

		        SimpleAudioEngine.SharedEngine.PlayBackgroundMusic(CCFileUtils.FullPathFromRelativePath(MUSIC_FILE), true);
		        break;
	        // stop background music
	        case 1:
		        SimpleAudioEngine.SharedEngine.StopBackgroundMusic();
		        break;
	        // pause background music
	        case 2:
		        SimpleAudioEngine.SharedEngine.PauseBackgroundMusic();
		        break;
	        // resume background music
	        case 3:
		        SimpleAudioEngine.SharedEngine.ResumeBackgroundMusic();
		        break;
	        // rewind background music
	        case 4:
		        SimpleAudioEngine.SharedEngine.RewindBackgroundMusic();
		        break;
	        // is background music playing
	        case 5:
		        if (SimpleAudioEngine.SharedEngine.IsBackgroundMusicPlaying())
		        {
			        CCLog.Log("background music is playing");
		        }
		        else
		        {
                    CCLog.Log("background music is not playing");
		        }
		        break;
	        // play effect
	        case 6:
		        m_nSoundId = SimpleAudioEngine.SharedEngine.PlayEffect(CCFileUtils.FullPathFromRelativePath(EFFECT_FILE));
		        break;
            // play effect
            case 7:
                m_nSoundId = SimpleAudioEngine.SharedEngine.PlayEffect(CCFileUtils.FullPathFromRelativePath(EFFECT_FILE), true);
                break;
            // stop effect
	        case 8:
		        SimpleAudioEngine.SharedEngine.StopEffect(m_nSoundId);
		        break;
	        // unload effect
	        case 9:
		        SimpleAudioEngine.SharedEngine.UnloadEffect(CCFileUtils.FullPathFromRelativePath(EFFECT_FILE));
		        break;
		        // add bakcground music volume
	        case 10:
		        SimpleAudioEngine.SharedEngine.BackgroundMusicVolume = SimpleAudioEngine.SharedEngine.BackgroundMusicVolume + 0.1f;
		        break;
		        // sub backgroud music volume
	        case 11:
		        SimpleAudioEngine.SharedEngine.BackgroundMusicVolume = SimpleAudioEngine.SharedEngine.BackgroundMusicVolume - 0.1f;
		        break;
		        // add effects volume
	        case 12:
		        SimpleAudioEngine.SharedEngine.EffectsVolume = SimpleAudioEngine.SharedEngine.EffectsVolume + 0.1f;
		        break;
		        // sub effects volume
	        case 13:
		        SimpleAudioEngine.SharedEngine.EffectsVolume = SimpleAudioEngine.SharedEngine.EffectsVolume - 0.1f;
		        break;
	        }
	
        }

        public override void TouchesBegan(List<CCTouch> pTouches, CCEvent pEvent)
        {
            CCTouch touch = pTouches.FirstOrDefault();

            m_tBeginPos = touch.LocationInView;
            m_tBeginPos = CCDirector.SharedDirector.ConvertToGl(m_tBeginPos);
        }

        public override void TouchesMoved(List<CCTouch> pTouches, CCEvent pEvent)
        {
            CCTouch touch = pTouches.FirstOrDefault();

	        CCPoint touchLocation = touch.LocationInView;	
	        touchLocation = CCDirector.SharedDirector.ConvertToGl( touchLocation );
	        float nMoveY = touchLocation.Y - m_tBeginPos.Y;

	        CCPoint curPos  = m_pItmeMenu.Position;
	        CCPoint nextPos = new CCPoint(curPos.X, curPos.Y + nMoveY);
	        CCSize winSize = CCDirector.SharedDirector.WinSize;
	        if (nextPos.Y < 0.0f)
	        {
		        m_pItmeMenu.Position = new CCPoint(0,0);
		        return;
	        }

	        if (nextPos.Y > ((m_nTestCount + 1)* LINE_SPACE - winSize.Height))
	        {
		        m_pItmeMenu.Position = new CCPoint(0, ((m_nTestCount + 1)* LINE_SPACE - winSize.Height));
		        return;
	        }

	        m_pItmeMenu.Position = nextPos;
	        m_tBeginPos = touchLocation;
        }


    }


    public class CocosDenshionTestScene : TestScene
    {
        protected override void NextTestCase()
        {
        }
        protected override void PreviousTestCase()
        {
        }
        protected override void RestTestCase()
        {
        }
        public override void runThisTest()
        {
	        CCLayer pLayer = new CocosDenshionTest();
	        AddChild(pLayer);

	        CCDirector.SharedDirector.ReplaceScene(this);
        }
    }
}