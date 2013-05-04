using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LD26.entities;

namespace LD26
{
    public class World
    {
        public int SubWidth { get { return (G.Width*SubPerc)/100; } }
        public int SubHeight { get { return (G.Height*SubPerc)/100; } }
        public bool DrawSubshit { get { return SubPerc > 1; }}

        public bool editorEnabled = false;
        public bool allowEditor = false;

        public Player player;

        public World()
        {
            allowEditor = true;
            editorEnabled = true;
        }

        public World(string[] level)
        {
            foreach (var line in level)
            {
                switch (line.First())
                {
                    case 'G': geometry.Add(Geometry.Parse(line.Substring(2))); break;
                    case 'E':
                        var e = Entity.Parse(this, line.Substring(2));
                        e.World = this;
                        entities.Add(e); break;
                    case 'N': Pathnodes.Add(Pathnode.Create(this, line.Substring(2))); break;
                    case 'L': Pathnode.ImportLinks(Pathnodes, line.Substring(2)); break;
                    case 'Q': Entity.LinkToNode(this, line.Substring(2)); break;
                    default: break;
                }
            }
            
            foreach (var en in entities)
            {
                en.World = this;
            }

            CreateMinimalGeomVB();

            player = entities.OfType<Player>().First();
        }

        private void CreateMinimalGeomVB()
        {
            var points = new List<VertexPositionColor>();
            
            var collections = geometry.Select(x => x.PointsLine);
            var pairs = new List<Pair>();
            foreach (var collection in collections)
            {
                var pair = new Pair();
                foreach (var p in collection)
                {
                    if (pair.First == null)
                    {
                        pair.First = p;
                    }
                    else
                    {
                        pair.Second = p;
                        pairs.Add(pair);
                        pair = new Pair();
                    }
                }
            }

            foreach (var pair in pairs)
            {
                pair.Sort();
            }

            pairs = pairs.Distinct().ToList();

            var vpcs = pairs.Select(x => new []{ new VertexPositionColor(x.First.Value, Color.White), new VertexPositionColor(x.Second.Value, Color.White)});
            foreach (var vpc in vpcs)
            {
                points.AddRange(vpc);
            }

            vbminimal = new VertexBuffer(gd, typeof(VertexPositionColor), points.Count, BufferUsage.WriteOnly);
            vbminimal.SetData(points.ToArray());
        }

        private VertexBuffer vbminimal;
        
        public Entity SelectedEntity { get; set; }

        List<Geometry> geometry = new List<Geometry>();
        public List<Entity> entities = new List<Entity>();
        List<Entity> entitiesToAdd = new List<Entity>();
        List<Vector3> points = new List<Vector3>();
        public List<Pathnode> Pathnodes = new List<Pathnode>();

        Type entityToPlaceType = typeof(Player);
        List<Type> entityTypes = new List<Type>() { typeof(Player), typeof(HorizontalDoor), typeof(VerticalDoor), typeof(Monster), typeof(Key), typeof(Checkpoint) };

        public int timer = 0;

        internal void Update()
        {
            //editorEnabled = true;

            timer++;

            if (player == null)
            {
                Camera3d.c.Update();
            }
#if debug
            if (allowEditor && RM.IsPressed(InputAction.RestartLevel))
            {
                if (editorEnabled)
                {

                    geometry.Clear();
                    entities.Clear();
                    entitiesToAdd.Clear();
                    Pathnodes.Clear();
                    SelectedNode = null;
                    SelectedEntity = null;
                }
                editorEnabled = true;
            }
#endif

            if (editorEnabled)
            {
                UpdateEditor();
            }
            else
            {
                if (!NextLevel)
                {
                    UpdateWorld();
                }
            }
#if debug
            if (allowEditor && RM.IsPressed(InputAction.EditorSave))
            {
                RM.SaveLevel(Export());
            }            
#endif
        }

        private void UpdateWorld()
        {
            foreach (var en in entities)
            {
                en.Update();
            }
            
            entities = entities.Where(e => !e.DeleteMe).Concat(entitiesToAdd).ToList();
            entitiesToAdd.Clear();

            if (RM.IsPressed(InputAction.EditorLeftClick) && IM.MousePos.X < 1024 && editorEnabled)
            {
                if (player != null)
                {
                    var ray = Camera3d.c.MouseRayCustom(player);
                    SelectEntity(entities.Where(e => e != player && e.Intersects(ray)).OrderBy(e => (e.Position.ToVector3() - player.Position.ToVector3()).Length()).FirstOrDefault());
                    if (SelectedEntity == null)
                    {
                        SelectedEntity = player;
                        player.Selected = true;
                    }
                }
                else
                {
                    var ray = Camera3d.c.MouseRay;
                    SelectEntity(entities.Where(e => e.Intersects(ray)).OrderBy(e => (e.Position.ToVector3() - Camera3d.c.position).Length()).FirstOrDefault());
                }
            }

            if (SelectedEntity != null)
            {
                SelectedEntity.UpdateHudShit();
            }
        }

        public void AddEntity(Entity e)
        {
            e.World = this;
            entitiesToAdd.Add(e);
        }

        private void UpdateEditor()
        {
            IM.SnapToCenter = false;
            G.g.IsMouseVisible = true;
            switch (editMode)
            {
                case EditMode.Geometry:
                    {
                        if (RM.IsPressed(InputAction.EditorLeftClick) && IM.MousePos.X < 1024)
                        {
                            var stg = SnapToGridPos;
                            points.Add(stg);
                        }

                        if (RM.IsPressed(InputAction.Action) && IM.MousePos.X < 1024)
                        {
                            if (points.Count == 3)
                            {
                                geometry.Add(new Geometry(points.ToList(), GeomType.Floor));
                                points.Clear();
                            }
                            else if (points.Count == 2)
                            {
                                geometry.Add(new Geometry(points.ToList(), GeomType.Wall));
                                points.Clear();
                            }
                            else
                            {
                                points.Clear();
                            }
                        }
                    }
                    break;
                case EditMode.Entities:
                    {
                        if (RM.IsPressed(InputAction.EditorLeftClick) && IM.MousePos.X < 1024)
                        {
                            var mPos = SnapToGridPos8;
                            if (entityToPlaceType == typeof(Player))
                            {
                                entities.Add(new Player(this, mPos.ToVector2Rounded()) { World = this });
                            }
                            if (entityToPlaceType == typeof(HorizontalDoor))
                            {
                                var door = new HorizontalDoor(mPos.ToVector2Rounded()) { World = this };
                                var node = new Pathnode(this) { Location = door.Position };
                                Pathnodes.Add(node);
                                door.Pathnode = node;
                                entities.Add(door);
                            }
                            if (entityToPlaceType == typeof(VerticalDoor))
                            {
                                var door = new VerticalDoor(mPos.ToVector2Rounded()) { World = this };
                                var node = new Pathnode(this) { Location = door.Position };
                                Pathnodes.Add(node);
                                door.Pathnode = node;
                                entities.Add(door);
                            }
                            if (entityToPlaceType == typeof(Monster))
                            {
                                entities.Add(new Monster(this, mPos.ToVector2Rounded()));
                            }
                            if (entityToPlaceType == typeof(Key))
                            {
                                entities.Add(new Key(mPos.ToVector2Rounded(), "red", 0));
                            }
                            if (entityToPlaceType == typeof(Checkpoint))
                            {
                                entities.Add(new Checkpoint(mPos.ToVector2Rounded()));
                            }
                        }
#if debug
                        if (RM.IsPressed(InputAction.AltFire) && IM.MousePos.X < 1024)
                        {
                            var ray = Camera3d.c.MouseRay;
                            SelectEntity(entities.Where(e => e.Intersects(ray)).OrderBy(e => (e.Position.ToVector3() - Camera3d.c.position).Length()).FirstOrDefault());
                        }
                        if (allowEditor && RM.IsPressed(InputAction.EditorSwitchType))
                        {
                            int index = entityTypes.IndexOf(entityToPlaceType);
                            index++;
                            if (index >= entityTypes.Count)
                            {
                                index = 0;
                            }
                            entityToPlaceType = entityTypes[index];
                        }

                        if (SelectedEntity != null)
                        {
                            SelectedEntity.UpdateHudShit();
                        }
#endif
                    }
                    break;
                case EditMode.Pathing:
                    {
                        if (RM.IsPressed(InputAction.EditorLeftClick) && IM.MousePos.X < 1024)
                        {
                            var mPos = SnapToGridPos8;
                            Pathnodes.Add(new Pathnode(this) { Location = mPos.ToVector2Rounded() });
                        }
#if debug
                        if (RM.IsPressed(InputAction.AltFire) && IM.MousePos.X < 1024)
                        {
                            var ray = Camera3d.c.MouseRay;
                            SelectNode(Pathnodes.Where(e => e.Intersects(ray)).OrderBy(e => (e.Location.ToVector3() - Camera3d.c.position).Length()).FirstOrDefault());
                        }
#endif

                        if (RM.IsPressed(InputAction.EditorLeftClick) && IM.MousePos.X > 1024 && SelectedNode != null)
                        {
                            SelectedNode.UpdateHudShit();
                        }
                    }
                    break;
            }
#if debug
            if (allowEditor && RM.IsPressed(InputAction.SwitchEditMode))
            {
                switch (editMode)
                {
                    case EditMode.Geometry: editMode = EditMode.Entities; break;
                    case EditMode.Entities: editMode = EditMode.Pathing; break;
                    case EditMode.Pathing: editMode = EditMode.Geometry; break;
                }
            }
#endif
        }

        private void SelectNode(Pathnode pathnode)
        {
            if (pathnode != null)
            {
                if (SelectedNode == null)
                {
                    SelectedNode = pathnode;
                    SelectedNode.Selected = true;
                }
                else
                {
                    if (pathnode != null)
                    {
                        pathnode.EditorLink(SelectedNode);
                    }
                    SelectedNode.Selected = false;
                    SelectedNode = null;
                }
            }
            else if (SelectedNode != null)
            {
                SelectedNode.Selected = false;
                SelectedNode = null;
            }
        }

        private RasterizerState wireRasterizer = new RasterizerState()
            {
                FillMode = FillMode.WireFrame,
                CullMode = CullMode.CullCounterClockwiseFace
            };

        internal void DrawMinimal()
        {
            if (editorEnabled)
            {
                DrawReal();
                return;
            }

            e.CurrentTechnique = e.Techniques["Wireframe"];
            e.Parameters["fogStart"].SetValue(1f);
            e.Parameters["fogEnd"].SetValue(80f);
            e.CurrentTechnique.Passes[0].Apply();
            gd.RasterizerState = wireRasterizer;

            if (player != null)
            {
                //Camera3d.c.ApplyCustom(player);
            }
            else
            {
                Camera3d.c.Apply(e);
            }

            gd.BlendState = BlendState.NonPremultiplied;
            e.Parameters["World"].SetValue(Matrix.Identity);
            e.Parameters["Texture"].SetValue(RM.GetTexture("white"));
            e.CurrentTechnique.Passes[0].Apply();

            gd.SetVertexBuffer(vbminimal);
            gd.DrawPrimitives(PrimitiveType.LineList, 0, vbminimal.VertexCount);

            e.Parameters["Texture"].SetValue(RM.GetTexture("white"));
            e.CurrentTechnique.Passes[0].Apply();
        }
        
        internal void DrawGeom()
        {
            if (player != null && !editorEnabled)
            {
                //Camera3d.c.ApplyCustom(player);
            }
            else
            {
                Camera3d.c.upDownRot = -MathHelper.PiOver2;
                Camera3d.c.Apply(e);
            }

            gd.BlendState = BlendState.NonPremultiplied;
            e.Parameters["World"].SetValue(Matrix.Identity);

            e.Parameters["Texture"].SetValue(RM.GetTexture("r"));
            e.CurrentTechnique.Passes[0].Apply();
            foreach (var geom in geometry)
            {
                gd.SetVertexBuffer(geom.vb);
                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, geom.vb.VertexCount / 3);
            }
            e.Parameters["Texture"].SetValue(RM.GetTexture("white"));

            e.CurrentTechnique.Passes[0].Apply();

            if (editorEnabled)
            {
                var mPos = Camera3d.c.MousePos;
                mPos = new Vector3((float)Math.Round(mPos.X), mPos.Y, (float)Math.Round(mPos.Z));
                if (editMode == EditMode.Geometry)
                {
                    mPos = SnapToGridPos;
                }
                if (editMode == EditMode.Entities || editMode == EditMode.Pathing)
                {
                    mPos = SnapToGridPos8;
                }

                e.Parameters["Texture"].SetValue(RM.GetTexture("white"));
                e.CurrentTechnique.Passes[0].Apply();

                if (editMode == EditMode.Pathing)
                {
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionNormalTexture[4]{ 
                        new VertexPositionNormalTexture(new Vector3(mPos.X-8f, 0,  mPos.Z-8f), new Vector3(1, 0, 0), new Vector2(0, 0)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X+8f, 0,  mPos.Z-8f), new Vector3(1, 0, 0), new Vector2(1, 0)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X-8f, 0,  mPos.Z+8f), new Vector3(1, 0, 0), new Vector2(0, 1)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X+8f, 0,  mPos.Z+8f), new Vector3(1, 0, 0), new Vector2(1, 1)),
                    }, 0, 2);
                }
                else
                {
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionNormalTexture[4]{ 
                        new VertexPositionNormalTexture(new Vector3(mPos.X-4f, 0,  mPos.Z-4f), new Vector3(1, 0, 0), new Vector2(0, 0)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X+4f, 0,  mPos.Z-4f), new Vector3(1, 0, 0), new Vector2(1, 0)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X-4f, 0,  mPos.Z+4f), new Vector3(1, 0, 0), new Vector2(0, 1)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X+4f, 0,  mPos.Z+4f), new Vector3(1, 0, 0), new Vector2(1, 1)),
                    }, 0, 2);
                }

                foreach (var p in points)
                {
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionNormalTexture[4]{ 
                        new VertexPositionNormalTexture(new Vector3(p.X-4f, 1,  p.Z-4f), new Vector3(1, 0, 0), new Vector2(0, 0)),
                        new VertexPositionNormalTexture(new Vector3(p.X+4f, 1,  p.Z-4f), new Vector3(1, 0, 0), new Vector2(1, 0)),
                        new VertexPositionNormalTexture(new Vector3(p.X-4f, 1,  p.Z+4f), new Vector3(1, 0, 0), new Vector2(0, 1)),
                        new VertexPositionNormalTexture(new Vector3(p.X+4f, 1,  p.Z+4f), new Vector3(1, 0, 0), new Vector2(1, 1)),
                    }, 0, 2);
                }

                if (editMode == EditMode.Pathing)
                {

                    e.CurrentTechnique = e.Techniques["VertexColor"];
                    e.CurrentTechnique.Passes[0].Apply();
                    foreach (var node in Pathnodes)
                    {
                        var p = node.Location;
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionColor[4]{ 
                        new VertexPositionColor(new Vector3(p.X-8f, 1,  p.Y-8f), node.Color),
                        new VertexPositionColor(new Vector3(p.X+8f, 1,  p.Y-8f), node.Color),
                        new VertexPositionColor(new Vector3(p.X-8f, 1,  p.Y+8f), node.Color),
                        new VertexPositionColor(new Vector3(p.X+8f, 1,  p.Y+8f), node.Color),
                        }, 0, 2);

                        foreach (var linked in node.LinkedNodes)
                        {
                            gd.DrawUserPrimitives(PrimitiveType.LineList, new VertexPositionColor[2] { new VertexPositionColor(new Vector3(p.X - 8f, 0, p.Y - 8f), node.Color),
                                new VertexPositionColor(new Vector3(linked.Location.X + 8f, 0, linked.Location.Y + 8f), node.Color),
                                }, 0, 1);
                        }
                    }
                }
            }
        }

        private void DrawEntities()
        {
            foreach (var ent in entities.OrderBy(x => (player.Position - x.Position).Length()).Reverse())
            {
                ent.Draw();
            }
        }

        internal void DrawReal()
        {
            e.CurrentTechnique = e.Techniques["Goggles"];
            //e.FogEnabled = false;

            e.CurrentTechnique.Passes[0].Apply();

            DrawGeom();
            DrawEntities();
        }

        public Vector3 SnapToGridPos
        {
            get
            {
                var mPos = Camera3d.c.MousePos;

                mPos /= 16;
                mPos = new Vector3((float)Math.Floor(mPos.X), mPos.Y, (float)Math.Floor(mPos.Z));
                mPos *= 16;

                return mPos;
            }
        }

        public Vector3 SnapToGridPos8
        {
            get
            {
                var mPos = Camera3d.c.MousePos;

                mPos /= 8;
                mPos = new Vector3((float)Math.Floor(mPos.X), mPos.Y, (float)Math.Floor(mPos.Z));
                mPos *= 8;

                return mPos;
            }
        }

        public Effect e { get { return G.g.VoidEffect; } }

        public GraphicsDevice gd { get { return G.g.GraphicsDevice; } }

        public string Export()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var geom in geometry)
            {
                sb.AppendLine(geom.Export());
            }
            foreach (var e in entities)
            {
                var exp = e.Export();
                if (exp != "")
                {
                    sb.AppendLine(exp);
                }
            }
            foreach (var n in Pathnodes)
            {
                sb.AppendLine(n.ExportNode());
            }
            foreach (var n in Pathnodes)
            {
                var exp = n.ExportLinks(Pathnodes);
                if (exp != "")
                {
                    sb.AppendLine(exp);
                }
            }
            foreach (var e in entities)
            {
                var exp = e.PathExport();
                if (exp != "")
                {
                    sb.AppendLine(exp);
                }
            }
            return sb.ToString();
        }

        public EditMode editMode = EditMode.Geometry;

        internal void DrawSprites()
        {
            //G.g.spriteBatch.Draw(RM.GetTexture("hudbar"), new Vector2(1024, 0), Color.White);
            if (editorEnabled)
            {
                G.g.spriteBatch.DrawString(G.g.font, editMode.ToString(), Vector2.Zero, Color.White);
                if (editMode == EditMode.Entities)
                {
                    G.g.spriteBatch.DrawString(G.g.font, entityToPlaceType.Name, new Vector2(0, 32), Color.White);
                }

                if (editMode == EditMode.Entities && SelectedEntity != null)
                {
                    SelectedEntity.DrawHUDInfo();
                }

                if (editMode == EditMode.Pathing && SelectedNode != null)
                {
                    SelectedNode.DrawHUDInfo();
                }
                G.g.spriteBatch.DrawString(G.g.font, SnapToGridPos8.ToString(), new Vector2(0, 96), Color.White);
            }
            else
            {
                if (SelectedEntity != null)
                {
                    SelectedEntity.DrawHUDInfo();
                }

                foreach (var ev in entities.OfType<Event>())
                {
                    ev.DrawHUDInfo();
                }
                player.DrawHUDInfo();
            }
        }

        public void SelectEntity(Entity e)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.Selected = false;
            }
            SelectedEntity = e;
            if (SelectedEntity != null)
            {
                SelectedEntity.Selected = true;
            }
        }

        public Pathnode SelectedNode { get; set; }

        public int SubPerc { get; set; }

        internal bool IsOnFloor(BoundingBox boundingBox)
        {
            List<Vector3> points = boundingBox.GetCorners().ToList();
            foreach (var geom in geometry)
            {
                geom.IsAboveFloor(points);
                if (!points.Any())
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsOnFloor(List<Vector3> points)
        {
            foreach (var geom in geometry)
            {
                geom.IsAboveFloor(points);
                if (!points.Any())
                {
                    return true;
                }
            }
            return false;
        }


        public bool NextLevel { get; set; }
    }

    public enum EditMode
    {
        Geometry,
        Entities,
        Pathing
    }
}
