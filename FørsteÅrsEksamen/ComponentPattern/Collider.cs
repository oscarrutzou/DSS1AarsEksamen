﻿using DoctorsDungeon.CommandPattern;
using DoctorsDungeon.GameManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DoctorsDungeon.ComponentPattern;

// Oscar
public class Collider : Component
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Texture2D texture;
    public int StartCollisionWidth { get; private set; }
    public int StartCollisionHeight { get; private set; } //If not set, use the sprite width and height

    private Vector2 offset;

    public Color DebugColor = Color.Red;

    public Rectangle CollisionBox
    {
        get
        {
            int width, height;
            Vector2 pos = GameObject.Transform.Position;
            if (animator != null && animator.CurrentAnimation != null)
            {
                width = StartCollisionWidth > 0 ? StartCollisionWidth : animator.CurrentAnimation.FrameDimensions;
                height = StartCollisionHeight > 0 ? StartCollisionHeight : animator.CurrentAnimation.FrameDimensions;
            }
            else
            {
                width = StartCollisionWidth > 0 ? StartCollisionWidth : spriteRenderer.Sprite.Width;
                height = StartCollisionHeight > 0 ? StartCollisionHeight : spriteRenderer.Sprite.Height;
            }

            width *= (int)GameObject.Transform.Scale.X;
            height *= (int)GameObject.Transform.Scale.Y;

            return new Rectangle
                (
                    (int)((pos.X - offset.X) - (width) / 2),
                    (int)((pos.Y - offset.Y) - (height) / 2),
                    width,
                    height
                );
        }
    }

    public Collider(GameObject gameObject) : base(gameObject)
    {
    }

    public override void Start()
    {
        animator = GameObject.GetComponent<Animator>();
        spriteRenderer = GameObject.GetComponent<SpriteRenderer>();
        texture = GlobalTextures.Textures[TextureNames.Pixel];
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!InputHandler.Instance.DebugMode) return;
        DrawRectangle(CollisionBox, DebugColor, spriteBatch);
    }

    /// <summary>
    /// Set custom collsionBox
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetCollisionBox(int width, int height)
    {
        StartCollisionWidth = width;
        StartCollisionHeight = height;
    }

    public void SetCollisionBox(int width, int height, Vector2 offset)
    {
        StartCollisionWidth = width;
        StartCollisionHeight = height;
        this.offset = offset;
    }

    /// <summary>
    /// Resets the custom collision box, and offset if it has been set
    /// </summary>
    public void ResetCustomCollsionBox()
    {
        offset = Vector2.Zero;
        StartCollisionHeight = 0;
        StartCollisionWidth = 0;
    }

    /// <summary>
    /// Draws a debug line around the rectangle
    /// </summary>
    /// <param name="collisionBox"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="vectorOffSet"></param>
    public void DrawRectangle(Rectangle collisionBox, Color color, SpriteBatch spriteBatch)
    {
        Vector2 colBoxPos = new Vector2(collisionBox.X, collisionBox.Y);

        int thickness = 1;
        Rectangle topLine = new Rectangle((int)colBoxPos.X, (int)colBoxPos.Y, collisionBox.Width, thickness);
        Rectangle bottomLine = new Rectangle((int)colBoxPos.X, (int)colBoxPos.Y + collisionBox.Height, collisionBox.Width, thickness);
        Rectangle rightLine = new Rectangle((int)colBoxPos.X + collisionBox.Width, (int)colBoxPos.Y, thickness, collisionBox.Height);
        Rectangle leftLine = new Rectangle((int)colBoxPos.X, (int)colBoxPos.Y, thickness, collisionBox.Height);

        spriteBatch.Draw(texture, topLine, null, color, 0, Vector2.Zero, spriteRenderer.SpriteEffects, 1);
        spriteBatch.Draw(texture, bottomLine, null, color, 0, Vector2.Zero, spriteRenderer.SpriteEffects, 1);
        spriteBatch.Draw(texture, rightLine, null, color, 0, Vector2.Zero, spriteRenderer.SpriteEffects, 1);
        spriteBatch.Draw(texture, leftLine, null, color, 0, Vector2.Zero, spriteRenderer.SpriteEffects, 1);
    }

    public static void DrawRectangleNoSprite(Rectangle rectangle, Color color, SpriteBatch spriteBatch)
    {
        Vector2 colBoxPos = new Vector2(rectangle.X, rectangle.Y);

        int thickness = 1;
        Rectangle topLine = new Rectangle((int)colBoxPos.X, (int)colBoxPos.Y, rectangle.Width, thickness);
        Rectangle bottomLine = new Rectangle((int)colBoxPos.X, (int)colBoxPos.Y + rectangle.Height, rectangle.Width, thickness);
        Rectangle rightLine = new Rectangle((int)colBoxPos.X + rectangle.Width, (int)colBoxPos.Y, thickness, rectangle.Height);
        Rectangle leftLine = new Rectangle((int)colBoxPos.X, (int)colBoxPos.Y, thickness, rectangle.Height);

        spriteBatch.Draw(GlobalTextures.Textures[TextureNames.Pixel], topLine, null, color, 0, Vector2.Zero, SpriteEffects.None, 1);
        spriteBatch.Draw(GlobalTextures.Textures[TextureNames.Pixel], bottomLine, null, color, 0, Vector2.Zero, SpriteEffects.None, 1);
        spriteBatch.Draw(GlobalTextures.Textures[TextureNames.Pixel], rightLine, null, color, 0, Vector2.Zero, SpriteEffects.None, 1);
        spriteBatch.Draw(GlobalTextures.Textures[TextureNames.Pixel], leftLine, null, color, 0, Vector2.Zero, SpriteEffects.None, 1);
    }
}