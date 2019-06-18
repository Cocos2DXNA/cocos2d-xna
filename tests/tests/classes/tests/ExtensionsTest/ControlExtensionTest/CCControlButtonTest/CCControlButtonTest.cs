using System;
using Cocos2D;
using Random = Cocos2D.CCRandom;

namespace tests.Extensions
{
    class CCControlButtonTest_HelloVariableSize : CCControlScene
	{
		public override bool Init()
		{
			if (base.Init())
			{
				CCSize screenSize = CCDirector.SharedDirector.WinSize;
        
				// Defines an array of title to create buttons dynamically
				var stringArray = new[] {
					"Hello",
					"Variable",
					"Size",
					"!"
				};
        
				CCNode layer = new CCNode ();
				AddChild(layer, 1);
        
				float total_width = 0, height = 0;
        
				// For each title in the array
				object pObj = null;
			    int i = 0;
				foreach(var title in stringArray)
				{
					// Creates a button with this string as title
					var button = standardButtonWithTitle(title);
				    if (i == 0)
				    {
				        button.Opacity = 50;
                        button.Color = new CCColor3B(0, 255, 0);
				    }
                    else if (i == 1)
                    {
                        button.Opacity = 200;
                        button.Color = new CCColor3B(0, 255, 0);
                    }
                    else if (i == 2)
                    {
                        button.Opacity = 100;
                        button.Color = new CCColor3B(0, 0, 255);
                    }

				    button.Position = new CCPoint (total_width + button.ContentSize.Width / 2, button.ContentSize.Height / 2);
					layer.AddChild(button);
            
					// Compute the size of the layer
					height = button.ContentSize.Height;
					total_width += button.ContentSize.Width;
				    i++;
				}

				layer.AnchorPoint = new CCPoint(0.5f, 0.5f);
				layer.ContentSize = new CCSize(total_width, height);
				layer.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
        
				// Add the black background
				var background = new CCScale9SpriteFile("extensions/buttonBackground");
				background.ContentSize = new CCSize(total_width + 14, height + 14);
				background.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
				AddChild(background);
				return true;
			}
			return false;
		}

		
		/** Creates and return a button with a default background and title color. */
		public CCControlButton standardButtonWithTitle(string title)
		{
			/** Creates and return a button with a default background and title color. */
			var backgroundButton = new CCScale9SpriteFile("extensions/button");
			var backgroundHighlightedButton = new CCScale9SpriteFile("extensions/buttonHighlighted");
    
			var titleButton = new CCLabelTTF(title, "Arial", 30);

			titleButton.Color = new CCColor3B(159, 168, 176);

            var button = new CCControlButton(titleButton, backgroundButton);
			button.SetBackgroundSpriteForState(backgroundHighlightedButton, CCControlState.Highlighted);
			button.SetTitleColorForState(CCTypes.CCWhite, CCControlState.Highlighted);
    
			return button;
		}


		public new static CCScene sceneWithTitle(string title)
		{
			var pScene = new CCScene();
			var controlLayer = new CCControlButtonTest_HelloVariableSize();
    		controlLayer.getSceneTitleLabel().Text = (title);
			pScene.AddChild(controlLayer);
			return pScene;
		}
	}

    class CCControlButtonTest_Inset : CCControlScene
    {
        public override bool Init()
        {
            if (base.Init())
            {
                CCSize screenSize = CCDirector.SharedDirector.WinSize;

                // Defines an array of title to create buttons dynamically
                var stringArray = new[] {
					"Inset",
					"Inset",
					"Inset"
				};

                CCNode layer = new CCNode ();
                AddChild(layer, 1);

                float total_width = 0, height = 0;

                // For each title in the array
                object pObj = null;
                foreach (var title in stringArray)
                {
                    // Creates a button with this string as title
                    CCControlButton button = insetButtonWithTitle(title, new CCRect(5, 5, 5, 5));
                    button.Position = new CCPoint(total_width + button.ContentSize.Width / 2, button.ContentSize.Height / 2);
                    layer.AddChild(button);

                    // Compute the size of the layer
                    height = button.ContentSize.Height;
                    total_width += button.ContentSize.Width;
                }

                layer.AnchorPoint = new CCPoint(0.5f, 0.5f);
                layer.ContentSize = new CCSize(total_width, height);
                layer.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);

                // Add the black background
                var background = new CCScale9SpriteFile("extensions/buttonBackground");
                background.ContentSize = new CCSize(total_width + 14, height + 14);
                background.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
                AddChild(background);
                return true;
            }
            return false;
        }


        /** Creates and return a button with a default background and title color. */
        public CCControlButton standardButtonWithTitle(string title)
        {
            /** Creates and return a button with a default background and title color. */
            var backgroundButton = new CCScale9SpriteFile("extensions/button");
            var backgroundHighlightedButton = new CCScale9SpriteFile("extensions/buttonHighlighted");

            var titleButton = new CCLabelTTF(title, "Arial", 30);

            titleButton.Color = new CCColor3B(159, 168, 176);

            var button = new CCControlButton(titleButton, backgroundButton);
            button.SetBackgroundSpriteForState(backgroundHighlightedButton, CCControlState.Highlighted);
            button.SetTitleColorForState(CCTypes.CCWhite, CCControlState.Highlighted);

            return button;
        }

        public CCControlButton insetButtonWithTitle(string title, CCRect inset)
        {
            /** Creates and return a button with a default background and title color. */
            var backgroundButton = new CCScale9SpriteFile("extensions/button");
            var backgroundHighlightedButton = new CCScale9SpriteFile("extensions/buttonHighlighted");
            backgroundButton.CapInsets = inset;
            backgroundHighlightedButton.CapInsets = inset;

            var titleButton = new CCLabelTTF(title, "Arial", 30);

            titleButton.Color = new CCColor3B(159, 168, 176);

            var button = new CCControlButton(titleButton, backgroundButton);
            button.SetBackgroundSpriteForState(backgroundHighlightedButton, CCControlState.Highlighted);
            button.SetTitleColorForState(CCTypes.CCWhite, CCControlState.Highlighted);

            return button;
        }

        public new static CCScene sceneWithTitle(string title)
        {
            var pScene = new CCScene();
            var controlLayer = new CCControlButtonTest_Inset();
            controlLayer.getSceneTitleLabel().Text = (title);
            pScene.AddChild(controlLayer);
            return pScene;
        }
    }

	class CCControlButtonTest_Event : CCControlScene
	{
		public override bool Init()
		{
			if (base.Init())
			{
				CCSize screenSize = CCDirector.SharedDirector.WinSize;

				// Add a label in which the button events will be displayed
				setDisplayValueLabel(new CCLabelTTF("No Event", "Arial", 32));
				m_pDisplayValueLabel.AnchorPoint = new CCPoint(0.5f, -1);
				m_pDisplayValueLabel.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
				AddChild(m_pDisplayValueLabel, 1);
        
				// Add the button
				var backgroundButton = new CCScale9SpriteFile("extensions/button");
				var backgroundHighlightedButton = new CCScale9SpriteFile("extensions/buttonHighlighted");
        
				var titleButton = new CCLabelTTF("Touch Me!", "Arial", 30);

				titleButton.Color = new CCColor3B(159, 168, 176);

                var controlButton = new CCControlButton(titleButton, backgroundButton);
				controlButton.SetBackgroundSpriteForState(backgroundHighlightedButton, CCControlState.Highlighted);
				controlButton.SetTitleColorForState(CCTypes.CCWhite, CCControlState.Highlighted);
        
				controlButton.AnchorPoint = new CCPoint(0.5f, 1);
				controlButton.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
				AddChild(controlButton, 1);

				// Add the black background
				var background = new CCScale9SpriteFile("extensions/buttonBackground");
				background.ContentSize = new CCSize(300, 170);
				background.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
				AddChild(background);
        
				// Sets up event handlers
				controlButton.AddTargetWithActionForControlEvent(this, touchDownAction, CCControlEvent.TouchDown);
				controlButton.AddTargetWithActionForControlEvent(this, touchDragInsideAction, CCControlEvent.TouchDragInside);
				controlButton.AddTargetWithActionForControlEvent(this, touchDragOutsideAction, CCControlEvent.TouchDragOutside);
				controlButton.AddTargetWithActionForControlEvent(this, touchDragEnterAction, CCControlEvent.TouchDragEnter);
				controlButton.AddTargetWithActionForControlEvent(this, touchDragExitAction, CCControlEvent.TouchDragExit);
				controlButton.AddTargetWithActionForControlEvent(this, touchUpInsideAction, CCControlEvent.TouchUpInside);
				controlButton.AddTargetWithActionForControlEvent(this, touchUpOutsideAction, CCControlEvent.TouchUpOutside);
				controlButton.AddTargetWithActionForControlEvent(this, touchCancelAction, CCControlEvent.TouchCancel);
				return true;
			}
			return false;
		}

	
		public void touchDownAction(object sender, CCControlEvent controlEvent)
		{
			m_pDisplayValueLabel.Text = ("Touch Down");
		}
		public void touchDragInsideAction(object sender, CCControlEvent controlEvent)
		{
			    m_pDisplayValueLabel.Text = ("Drag Inside");
		}

		public void touchDragOutsideAction(object sender, CCControlEvent controlEvent)
		{
			m_pDisplayValueLabel.Text = ("Drag Outside");
		}

		public void touchDragEnterAction(object sender, CCControlEvent controlEvent)
		{
			m_pDisplayValueLabel.Text = ("Drag Enter");
		}

		public void touchDragExitAction(object sender, CCControlEvent controlEvent)
		{
			m_pDisplayValueLabel.Text = ("Drag Exit");
		}
		
		public void touchUpInsideAction(object sender, CCControlEvent controlEvent)
		{
			m_pDisplayValueLabel.Text = ("Touch Up Inside.");
		}
		
		public void touchUpOutsideAction(object sender, CCControlEvent controlEvent)
		{
			m_pDisplayValueLabel.Text = ("Touch Up Outside.");
		}

		public void touchCancelAction(object sender, CCControlEvent controlEvent)
		{
			m_pDisplayValueLabel.Text = ("Touch Cancel");
		}
	
		private CCLabelTTF m_pDisplayValueLabel; 
		public virtual CCLabelTTF getDisplayValueLabel() { return m_pDisplayValueLabel; } 
		public virtual void setDisplayValueLabel(CCLabelTTF var)   
		{ 
			if (m_pDisplayValueLabel != var) 
			{ 
				m_pDisplayValueLabel = var; 
			} 
		} 
		
		public new static CCScene sceneWithTitle(string title)
		{
			var pScene = new CCScene();
			var controlLayer = new CCControlButtonTest_Event();
			if (controlLayer != null)
			{
				controlLayer.getSceneTitleLabel().Text = (title);
				pScene.AddChild(controlLayer);
			}
			return pScene;
		}
	}

	class CCControlButtonTest_Styling : CCControlScene
	{
		public override bool Init()
		{
			if (base.Init())
			{
				CCSize screenSize = CCDirector.SharedDirector.WinSize;

				var layer = new CCNode ();
				AddChild(layer, 1);
        
				int space = 10; // px
        
				float max_w = 0, max_h = 0;
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; j < 3; j++)
					{
						
						
						// Add the buttons
						var button = standardButtonWithTitle(CCRandom.Next(30).ToString());
						button.SetAdjustBackgroundImage(false);  // Tells the button that the background image must not be adjust
															// It'll use the prefered size of the background image
                        button.Position = new CCPoint(button.ContentSize.Width / 2 + (button.ContentSize.Width + space) * i,
                                               button.ContentSize.Height / 2 + (button.ContentSize.Height + space) * j);
						layer.AddChild(button);

                        max_w = Math.Max(button.ContentSize.Width * (i + 1) + space * i, max_w);
                        max_h = Math.Max(button.ContentSize.Height * (j + 1) + space * j, max_h);
					}
				}
        
				layer.AnchorPoint = new CCPoint (0.5f, 0.5f);
				layer.ContentSize = new CCSize(max_w, max_h);
				layer.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
        
				// Add the black background
				var backgroundButton = new CCScale9SpriteFile("extensions/buttonBackground");
				backgroundButton.ContentSize = new CCSize(max_w + 14, max_h + 14);
				backgroundButton.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
				AddChild(backgroundButton);
				return true;
			}
			return false;
		}




		public CCControlButton standardButtonWithTitle(string title)
		{
			/** Creates and return a button with a default background and title color. */
			var backgroundButton = new CCScale9SpriteFile("extensions/button");
			backgroundButton.PreferredSize = new CCSize(55, 55);  // Set the prefered size
			var backgroundHighlightedButton = new CCScale9SpriteFile("extensions/buttonHighlighted");
			backgroundHighlightedButton.PreferredSize = new CCSize(55, 55);  // Set the prefered size
    
			var titleButton = new CCLabelTTF(title, "Arial", 30);

			titleButton.Color = new CCColor3B(159, 168, 176);

            var button = new CCControlButton(titleButton, backgroundButton);
			button.SetBackgroundSpriteForState(backgroundHighlightedButton, CCControlState.Highlighted);
			button.SetTitleColorForState(CCTypes.CCWhite, CCControlState.Highlighted);
    
			return button;
		}

		public new static CCScene sceneWithTitle(string title)
		{
			var pScene = new CCScene();
			var controlLayer = new CCControlButtonTest_Styling();
			if (controlLayer != null)
			{
				controlLayer.getSceneTitleLabel().Text = (title);
				pScene.AddChild(controlLayer);
			}
			return pScene;
		}
	}

    class CCControlButtonTest_Issue448 : CCControlScene
    {
        private CCTexture2D _TheTexture;
        private const int kTheLayer = 1001;
        private const int kTheButtons = 1002;
        private float max_w = 0f, max_h = 0f;

        public override void OnEnter()
        {
            base.OnEnter();
            ScheduleUpdate();
        }

        private float elapsedTime = 0f;
        public override void Update(float dt)
        {
            base.Update(dt);
            elapsedTime += dt;
            if (elapsedTime > 3f)
            {
                if (_TheTexture != null)
                {
                    _TheTexture.XNATexture.Dispose();
                    _TheTexture = null;
                }
                CreateTheButtons();
                elapsedTime = 0f;
                var title = new CCLabelTTF("Refreshing the buttons", "Arial", 30);
                AddChild(title, 25);
                title.Position = ContentSize.Center;
                title.RunAction(new CCFadeOut(1f));
            }
        }

        /*
         * This is a test of the texture reinit on the control button.
         */
        public override bool Init()
        {
            if (base.Init())
            {
                CCSize screenSize = CCDirector.SharedDirector.WinSize;

                var layer = new CCNode();
                AddChild(layer, 1, kTheLayer);

                CreateTheButtons();

                layer.AnchorPoint = new CCPoint(0.5f, 0.5f);
                layer.ContentSize = new CCSize(max_w, max_h);
                layer.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);

                // Add the black background
                var backgroundButton = new CCScale9SpriteFile("extensions/buttonBackground");
                backgroundButton.ContentSize = new CCSize(max_w + 14, max_h + 14);
                backgroundButton.Position = new CCPoint(screenSize.Width / 2.0f, screenSize.Height / 2.0f);
                AddChild(backgroundButton);
                return true;
            }
            return false;
        }


        private void CreateTheButtons()
        {
            CCNode theLayer = GetChildByTag(kTheLayer);
            theLayer.RemoveAllChildrenByTag(kTheButtons);
            int space = 10; // px

            max_w = 0f;
            max_h = 0f;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {


                    // Add the buttons
                    var button = standardButtonWithTitle(CCRandom.Next(30).ToString());
                    button.SetAdjustBackgroundImage(false);  // Tells the button that the background image must not be adjust
                    // It'll use the prefered size of the background image
                    button.Position = new CCPoint(button.ContentSize.Width / 2 + (button.ContentSize.Width + space) * i,
                                           button.ContentSize.Height / 2 + (button.ContentSize.Height + space) * j);
                    theLayer.AddChild(button, kTheButtons);

                    max_w = Math.Max(button.ContentSize.Width * (i + 1) + space * i, max_w);
                    max_h = Math.Max(button.ContentSize.Height * (j + 1) + space * j, max_h);
                }
            }
        }

        public CCControlButton standardButtonWithTitle(string title)
        {
            _TheTexture = CCTextureCache.SharedTextureCache.AddImage("extensions/button");
            /** Creates and return a button with a default background and title color. */
            var backgroundButton = new CCScale9SpriteFile("extensions/button");
            backgroundButton.PreferredSize = new CCSize(55, 55);  // Set the prefered size
            var backgroundHighlightedButton = new CCScale9SpriteFile("extensions/buttonHighlighted");
            backgroundHighlightedButton.PreferredSize = new CCSize(55, 55);  // Set the prefered size

            var titleButton = new CCLabelTTF(title, "Arial", 30);

            titleButton.Color = new CCColor3B(159, 168, 176);

            var button = new CCControlButton(titleButton, backgroundButton);
            button.SetBackgroundSpriteForState(backgroundHighlightedButton, CCControlState.Highlighted);
            button.SetTitleColorForState(CCTypes.CCWhite, CCControlState.Highlighted);

            return button;
        }

        public new static CCScene sceneWithTitle(string title)
        {
            var pScene = new CCScene();
            var controlLayer = new CCControlButtonTest_Issue448();
            if (controlLayer != null)
            {
                controlLayer.getSceneTitleLabel().Text = (title);
                pScene.AddChild(controlLayer);
            }
            return pScene;
        }
    }
}