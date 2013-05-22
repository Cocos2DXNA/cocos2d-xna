using System;
using Cocos2D;

namespace tests.Extensions
{
	public class TableViewTestLayer : CCLayer, CCTableViewDataSource, CCTableViewDelegate
	{
		public static void runTableViewTest()
		{
			var pScene = new CCScene();
			var pLayer = new TableViewTestLayer();
			pLayer.Init();
			pScene.AddChild(pLayer);
			CCDirector.SharedDirector.ReplaceScene(pScene);
		}

		public override bool Init()
		{
			if (!base.Init())
				return false;

			var winSize = CCDirector.SharedDirector.WinSize;

			var tableView = new CCTableView(this, new CCSize(250, 60));
			tableView.Direction = CCScrollViewDirection.Horizontal;
			tableView.Position = new CCPoint(20,winSize.Height/2-30);
			tableView.Delegate = this;
			this.AddChild(tableView);
			tableView.ReloadData();

			tableView = new CCTableView(this, new CCSize(60, 280));
			tableView.Direction = CCScrollViewDirection.Vertical;
			tableView.Position = new CCPoint(winSize.Width - 150, winSize.Height/2 - 120);
			tableView.Delegate = this;
			tableView.VerticalFillOrder = CCTableViewVerticalFillOrder.FillTopDown;
			this.AddChild(tableView);
			tableView.ReloadData();

			// Back Menu
			var itemBack = new CCMenuItemFont("Back", toExtensionsMainLayer);
			itemBack.Position = new CCPoint(winSize.Width - 50, 25);
			var menuBack = new CCMenu(itemBack);
			menuBack.Position = CCPoint.Zero;
			AddChild(menuBack);

			return true;
		}
   
		public void toExtensionsMainLayer(object sender)
		{
			var pScene = new ExtensionsTestScene();
			pScene.runThisTest();
		}

		//CREATE_FUNC(TableViewTestLayer);
    
		public virtual void ScrollViewDidScroll(CCScrollView view)
		{
		}

		public virtual void ScrollViewDidZoom(CCScrollView view)
		{
		}

		public virtual void TableCellTouched(CCTableView table, CCTableViewCell cell)
		{
            CCLog.Log("cell touched at index: {0}", cell.Index);
		}

		public virtual CCSize CellSizeForTable(CCTableView table)
		{
			return new CCSize(60, 60);
		}

		public virtual CCTableViewCell TableCellAtIndex(CCTableView table, int idx)
		{
			string str = idx.ToString();
			var cell = table.DequeueCell();
			
			if (cell == null) {
				cell = new CustomTableViewCell();
				var sprite = new CCSprite("Images/Icon");
				sprite.AnchorPoint = CCPoint.Zero;
				sprite.Position = new CCPoint(0, 0);
				cell.AddChild(sprite);

				var label = new CCLabelTTF(str, "Helvetica", 20.0f);
				label.Position = CCPoint.Zero;
				label.AnchorPoint = CCPoint.Zero;
				label.Tag = 123;
				cell.AddChild(label);
			}
			else
			{
				var label = (CCLabelTTF)cell.GetChildByTag(123);
				label.Label = (str);
			}


			return cell;
		}

		public virtual int NumberOfCellsInTableView(CCTableView table)
		{
			return 20;
		}
	}
}