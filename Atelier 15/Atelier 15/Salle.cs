using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AtelierXNA
{
    public class Salle : PrimitiveDeBaseAnimée
    {
        const int NB_TRIANGLES_PAR_TUILE = 2;
        const int NB_SOMMETS_PAR_TRIANGLE = 3;
        const int NB_NIVEAUX_TEXTURE = 2;
        const float MAX_COULEUR = 256f;

        Vector3 Étendue { get; set; }
        string NomCarteTerrain { get; set; }
        string NomTextures { get; set; }

        BasicEffect EffetDeBase { get; set; }
        RessourcesManager<Texture2D> GestionnaireDeTextures { get; set; }
        Texture2D CarteTerrain { get; set; }
        Texture2D TextureTerrain { get; set; }
        Vector3 Origine { get; set; }
        Vector2 DeltaTexture { get; set; }
        Color[] DataTexture { get; set; }
        VertexPositionTexture[] Sommets { get; set; }
        VertexPositionTexture[] SommetsMurs { get; set; }
        VertexPositionTexture[] SommetsPlafond { get; set; }
        Vector3[,] PtsSommets { get; set; }
        Vector3[,] PtsSommetsPlafond { get; set; }
        Vector3[,] PtsSommetsMurs { get; set; }
        Vector2 Delta { get; set; }
        int NbRangées { get; set; }
        int NbColonnes { get; set; }
        float HauteurMax { get; set; }
        int NbTexels { get; set; }


        public Salle(Game jeu, float homothétieInitiale, Vector3 rotationInitiale, Vector3 positionInitiale,
                       Vector3 étendue, string nomCarteTerrain, string nomTextures, float intervalleMAJ)
           : base(jeu, homothétieInitiale, rotationInitiale, positionInitiale, intervalleMAJ)
        {
            Étendue = étendue;
            NomCarteTerrain = nomCarteTerrain;
            NomTextures = nomTextures;
        }

        public override void Initialize()
        {
            GestionnaireDeTextures = Game.Services.GetService(typeof(RessourcesManager<Texture2D>)) as RessourcesManager<Texture2D>;
            InitialiserDonnéesCarte();
            InitialiserDonnéesTexture();
            NbColonnes = CarteTerrain.Width - 1;
            NbRangées = CarteTerrain.Height - 1;
            HauteurMax = Étendue.Y;
            Delta = new Vector2(Étendue.X / NbColonnes, Étendue.Z / NbRangées);
            NbSommets = NbColonnes * NbRangées * NB_TRIANGLES_PAR_TUILE * NB_SOMMETS_PAR_TRIANGLE;
            NbTriangles = NB_TRIANGLES_PAR_TUILE * NbRangées * NbColonnes;
            Origine = new Vector3(-Étendue.X / 2, 0, Étendue.Z / 2); //pour centrer la primitive au point (0,0,0)
            AllouerTableaux();
            CréerTableauPoints();
            base.Initialize();
        }
        void InitialiserDonnéesCarte()
        {
            CarteTerrain = GestionnaireDeTextures.Find(NomCarteTerrain);
            NbTexels = CarteTerrain.Width * CarteTerrain.Height;
            DataTexture = new Color[NbTexels];
            CarteTerrain.GetData(DataTexture);
        }
        void InitialiserDonnéesTexture()
        {
            TextureTerrain = GestionnaireDeTextures.Find(NomTextures);
            DeltaTexture = new Vector2(1, (float)1 / NB_NIVEAUX_TEXTURE);
        }
        void AllouerTableaux()
        {
            PtsSommets = new Vector3[NbColonnes + 1, NbRangées + 1];
            Sommets = new VertexPositionTexture[NbSommets];
            
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            EffetDeBase = new BasicEffect(GraphicsDevice);
            InitialiserParamètresEffetDeBase();
        }

        void InitialiserParamètresEffetDeBase()
        {
            EffetDeBase.TextureEnabled = true;
            EffetDeBase.Texture = TextureTerrain;
        }

        private void CréerTableauPoints()
        {
            for (int j = 0; j <= NbRangées; ++j)
            {
                int texel = NbTexels - (j + 1) * CarteTerrain.Width;
                for (int i = 0; i <= NbColonnes; ++i)
                {
                    float hauteur = CalculerHauteur(texel);
                    PtsSommets[i, j] = new Vector3(Origine.X + i * Delta.X, hauteur, Origine.Z - j * Delta.Y);
                    ++texel;
                }
            }
        }

        float CalculerHauteur(int texel)
        {
            return DataTexture[texel].B / MAX_COULEUR * HauteurMax;
        }
        protected override void InitialiserSommets()
        {
            int NoSommet = -1;
            for (int j = 0; j < NbRangées; ++j)
            {
                float facteur_x = 0;
                for (int i = 0; i < NbColonnes; ++i)
                {
                    int facteur = 0;
                    float hauteur = PtsSommets[i, j].Y + PtsSommets[i + 1, j].Y + PtsSommets[i, j + 1].Y;

                    if(hauteur != 0)
                    {
                        facteur = 1;
                    }
                    Vector2 texture = new Vector2(0, facteur * DeltaTexture.Y);

                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j], texture + new Vector2(facteur_x,0));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j + 1], texture + new Vector2(facteur_x, DeltaTexture.Y));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j], texture + new Vector2(facteur_x + DeltaTexture.X, 0));

                    hauteur = PtsSommets[i, j + 1].Y + PtsSommets[i + 1, j].Y + PtsSommets[i + 1, j + 1].Y;
                    if (hauteur != 0)
                    {
                        facteur = 1;
                    }
                    texture = new Vector2(0, facteur * DeltaTexture.Y);

                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j], texture + new Vector2(facteur_x + DeltaTexture.X, 0));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i, j + 1], texture + new Vector2(facteur_x, DeltaTexture.Y));
                    Sommets[++NoSommet] = new VertexPositionTexture(PtsSommets[i + 1, j + 1], texture + new Vector2(facteur_x + DeltaTexture.X, DeltaTexture.Y));

                    //facteur_x += 1 / 15;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            EffetDeBase.World = GetMonde();
            EffetDeBase.View = CaméraJeu.Vue;
            EffetDeBase.Projection = CaméraJeu.Projection;
            foreach (EffectPass passeEffet in EffetDeBase.CurrentTechnique.Passes)
            {
                passeEffet.Apply();
                GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, Sommets, 0, NbTriangles);
            }
        }
    }
}
