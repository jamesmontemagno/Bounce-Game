using System;
using System.Collections.Generic;
using CocosSharp;
using Microsoft.Xna.Framework;
using CocosDenshion;

namespace BounceTest
{
  public class IntroLayer : CCLayerColor
  {
    CCLabelTtf label;
    CCSprite paddleSprite, ballSprite;
    public IntroLayer()
    {

      // create and initialize a Label
      label = new CCLabelTtf("0", "MarkerFelt", 22);
      label.AnchorPoint = CCPoint.AnchorMiddleTop;
      label.PositionY = VisibleBoundsWorldspace.MaxY - 20;

      // add the label as a child to this Layer
      AddChild(label);

      paddleSprite = new CCSprite("paddle");
      paddleSprite.PositionX = 100;
      paddleSprite.PositionY = 100;

      AddChild(paddleSprite);

      ballSprite = new CCSprite("ball");
      ballSprite.PositionX = 320;
      ballSprite.PositionY = 640;

      AddChild(ballSprite);

      // setup our color for the background
      Color = new CCColor3B(CCColor4B.Blue);
      Opacity = 255;
      
    }
    protected override void AddedToScene()
    {
      base.AddedToScene();

      // Use the bounds to layout the positioning of our drawable assets
      var bounds = VisibleBoundsWorldspace;

      // position the label on the center of the screen
      label.PositionY = bounds.MaxY - 20;
      label.PositionX = 100;


      // Register for touch events
      var touchListener = new CCEventListenerTouchAllAtOnce();
      touchListener.OnTouchesMoved = OnTouchesMoved;
      AddEventListener(touchListener, this);

      Schedule(RunGameLogic);
    }

    void OnTouchesMoved(List<CCTouch> touches, CCEvent touchEvent)
    {
      if (touches.Count > 0)
      {

        paddleSprite.RunAction(new CCMoveTo(.1f, 
          new CCPoint(touches[0].Location.X, paddleSprite.PositionY)));
        // Perform touch handling here
      }
    }


    float ballXVelocity;
    float ballYVelocity;
    // How much to modify the ball's y velocity per second:
    const float gravity = 140;

    int score = 0;
    void RunGameLogic(float frameTimeInSeconds)
    {
      // This is a linear approximation, so not 100% accurate
      ballYVelocity += frameTimeInSeconds * -gravity;

      ballSprite.PositionX += ballXVelocity * frameTimeInSeconds;
      ballSprite.PositionY += ballYVelocity * frameTimeInSeconds;

      
      // New Code:

      // Check if the two CCSprites overlap...
      bool doesBallOverlapPaddle = ballSprite.BoundingBoxTransformedToParent.IntersectsRect(
          paddleSprite.BoundingBoxTransformedToParent);
      // ... and if the ball is moving downward.
      bool isMovingDownward = ballYVelocity < 0;
      if (doesBallOverlapPaddle && isMovingDownward)
      {
        // First let's invert the velocity:
        ballYVelocity *= -1;
        // Then let's assign a random value to the ball's x velocity:
        const float minXVelocity = -300;
        const float maxXVelocity = 300;
        ballXVelocity = CCRandom.GetRandomFloat(minXVelocity, maxXVelocity);

        score++;
        label.Text = "Score: " + score;

        var effect = new CCParticleExplosion(ballSprite.Position)
        {
          AutoRemoveOnFinish = true,
          EmissionRate = 100
        };
        AddChild(effect);

        CCSimpleAudioEngine.SharedEngine.PlayEffect("sounds/tap");
      }

      // First let’s get the ball position:   
      float ballRight = ballSprite.BoundingBoxTransformedToParent.MaxX;
      float ballLeft = ballSprite.BoundingBoxTransformedToParent.MinX;

      // Then let’s get the screen edges
      float screenRight = VisibleBoundsWorldspace.MaxX;
      float screenLeft = VisibleBoundsWorldspace.MinX;

      // Check if the ball is either too far to the right or left:    
      bool shouldReflectXVelocity =
          (ballRight > screenRight && ballXVelocity > 0) ||
          (ballLeft < screenLeft && ballXVelocity < 0);

      if (shouldReflectXVelocity)
      {
        ballXVelocity *= -1;
      }

      if(ballSprite.PositionY < VisibleBoundsWorldspace.MinY)
      {
        ballSprite.PositionY = VisibleBoundsWorldspace.MaxY / 2.0f;
        score = 0;
        label.Text = "Score: 0";
        ballXVelocity = 0;
        ballYVelocity = 0;
      }
    }
  }
}

