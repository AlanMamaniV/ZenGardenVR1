using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZenGardenVR.Editor
{
    /// <summary>
    /// Generador automatizado de entorno 3D para Realidad Virtual en Unity.
    /// Crea un Jardín Zen y Templo Meditativo a escala real 1:1 en metros,
    /// con jerarquía estructurada de objetos primitivos, materiales y texturas procedurales,
    /// iluminación con sombras suaves, rig de cámara VR con TrackedPoseDriver a 1.65m,
    /// animación suave por script y audio ambiental.
    /// </summary>
    public static class GenerarEscenaVR
    {
        private const string RUTA_BASE = "Assets/ZenGarden_VR";
        private const string RUTA_TEXTURAS = RUTA_BASE + "/Texturas";
        private const string RUTA_MATERIALES = RUTA_BASE + "/Materiales";
        private const string RUTA_AUDIO = RUTA_BASE + "/Audio";

        // Estructura contenedora de materiales generados
        private class MaterialesJardin
        {
            public Material arenaZen;
            public Material maderaTorii;
            public Material maderaOscura;
            public Material piedraMusgo;
            public Material aguaZen;
            public Material papelFarola;
            public Material oroBronce;
            public Material hojasLoto;
            public Material cristalZen;
        }

        [MenuItem("Herramientas VR/Generar Escena Completa")]
        public static void GenerarEscenaCompleta()
        {
            if (!Application.isBatchMode)
                EditorUtility.DisplayProgressBar("Generador VR", "Preparando carpetas y recursos...", 0.1f);

            try
            {
                // 1. Asegurar directorios de activos
                AsegurarCarpetas();

                // 2. Generar y guardar texturas procedurales y materiales
                if (!Application.isBatchMode)
                    EditorUtility.DisplayProgressBar("Generador VR", "Generando texturas y materiales...", 0.3f);
                MaterialesJardin mats = GenerarMaterialesYTexturas();

                // 3. Generar archivo de audio ambiental (campana / cuenco tibetano zen en WAV)
                if (!Application.isBatchMode)
                    EditorUtility.DisplayProgressBar("Generador VR", "Generando audio ambiental sintético...", 0.5f);
                AudioClip clipAmbiental = GenerarAudioClipAmbiental();

                // 4. Limpiar objetos previos si ya existía una generación anterior
                GameObject raizAnterior = GameObject.Find("[Escena VR] Jardin Zen Meditativo");
                if (raizAnterior != null)
                {
                    Undo.DestroyObjectImmediate(raizAnterior);
                }

                GameObject rigAnterior = GameObject.Find("[VR Rig] Jugador");
                if (rigAnterior != null)
                {
                    Undo.DestroyObjectImmediate(rigAnterior);
                }

                // 5. Construir jerarquía 3D completa
                if (!Application.isBatchMode)
                    EditorUtility.DisplayProgressBar("Generador VR", "Construyendo geometría 3D y jerarquía...", 0.7f);
                GameObject raizEscena = new GameObject("[Escena VR] Jardin Zen Meditativo");
                Undo.RegisterCreatedObjectUndo(raizEscena, "Generar Escena VR Zen");

                ConstruirSueloYLimites(raizEscena.transform, mats);
                ConstruirPortalTorii(raizEscena.transform, mats);
                ConstruirTemploPagoda(raizEscena.transform, mats);
                ConstruirAltarMeditativo(raizEscena.transform, mats);
                ConstruirFarolasToro(raizEscena.transform, mats);
                ConstruirEstanqueLoto(raizEscena.transform, mats);
                ConstruirIslaRocasZen(raizEscena.transform, mats);
                ConstruirCaminoPiedras(raizEscena.transform, mats);
                ConstruirIluminacion(raizEscena.transform);
                ConstruirEmisorAudio(raizEscena.transform, clipAmbiental);

                // 6. Configurar Sistema y Cámara VR (Escala 1:1, Altura 1.65m, TrackedPoseDriver)
                if (!Application.isBatchMode)
                    EditorUtility.DisplayProgressBar("Generador VR", "Configurando Cámara VR y Head Tracking...", 0.9f);
                ConfigurarCamaraVR();

                // 7. Enfocar escena en la vista de Scene del editor
                Selection.activeGameObject = raizEscena;
                SceneView.FrameLastActiveSceneView();

                // Marcar y guardar escena activa como Assets/Scenes/ZenGardenVR.unity
                var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
                string rutaEscenaVR = "Assets/Scenes/ZenGardenVR.unity";
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene, rutaEscenaVR);

                Debug.Log("<color=#4CAF50><b>[Generador VR]</b> ¡Escena de Jardín Zen generada y guardada exitosamente en " + rutaEscenaVR + "!</color> Escala 1:1, jerarquía de primitivos, materiales mapeados, iluminación suave y TrackedPoseDriver listos.");
                
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Generador VR", "¡Escena de Realidad Virtual generada con éxito!\n\n- Entorno: Jardín Zen y Templo Meditativo.\n- Escala: 1:1 en metros.\n- Cámara VR: 1.65m de altura de ojos con TrackedPoseDriver.\n- Jerarquía: Padres vacíos para cada estructura compuesta.\n- Texturas procedurales y materiales asignados.\n- Iluminación suave y linternas con Point Lights.\n- Animación suave (RotadorSuave) y Audio ambiental incluidos.", "Entendido");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Generador VR] Error durante la generación: {ex.Message}\n{ex.StackTrace}");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Error en Generador VR", "Ocurrió un error al generar la escena: " + ex.Message, "Cerrar");
                }
            }
            finally
            {
                if (!Application.isBatchMode)
                    EditorUtility.ClearProgressBar();
            }
        }

        #region Creación de Carpetas y Recursos

        private static void AsegurarCarpetas()
        {
            if (!AssetDatabase.IsValidFolder(RUTA_BASE))
                AssetDatabase.CreateFolder("Assets", "ZenGarden_VR");
            if (!AssetDatabase.IsValidFolder(RUTA_TEXTURAS))
                AssetDatabase.CreateFolder(RUTA_BASE, "Texturas");
            if (!AssetDatabase.IsValidFolder(RUTA_MATERIALES))
                AssetDatabase.CreateFolder(RUTA_BASE, "Materiales");
            if (!AssetDatabase.IsValidFolder(RUTA_AUDIO))
                AssetDatabase.CreateFolder(RUTA_BASE, "Audio");
        }

        private static Shader ObtenerShaderCompatible()
        {
            // Soporte universal: URP Lit primero (adecuado para Unity 6 URP), fallback a Standard o Diffuse
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Mobile/Diffuse");
            if (s == null) s = Shader.Find("Diffuse");
            return s;
        }

        private static Material CrearOGuardarMaterial(string nombre, Texture2D textura, Color colorBase, float metallic, float smoothness, Vector2 tiling, bool esEmisivo = false, Color? colorEmision = null)
        {
            string ruta = $"{RUTA_MATERIALES}/{nombre}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
            if (mat == null)
            {
                mat = new Material(ObtenerShaderCompatible());
                AssetDatabase.CreateAsset(mat, ruta);
            }
            else
            {
                mat.shader = ObtenerShaderCompatible();
            }

            mat.name = nombre;

            // Configurar color base compatible con URP (_BaseColor) y Built-in (_Color)
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", colorBase);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", colorBase);

            // Asignar textura con repetición (tiling) compatible con URP (_BaseMap) y Built-in (_MainTex)
            if (textura != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", textura);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", textura);

                mat.mainTextureScale = tiling;
                if (mat.HasProperty("_BaseMap")) mat.SetTextureScale("_BaseMap", tiling);
                if (mat.HasProperty("_MainTex")) mat.SetTextureScale("_MainTex", tiling);
            }

            // Propiedades PBR (Metallic y Smoothness / Glossiness)
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            // Configuración de emisión para lámparas y orbe espiritual
            if (esEmisivo)
            {
                Color emision = colorEmision ?? (colorBase * 1.6f);
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emision);
                if (textura != null && mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", textura);
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Texture2D GuardarTexturaPNG(string nombre, Texture2D tex)
        {
            byte[] bytes = tex.EncodeToPNG();
            string ruta = $"{RUTA_TEXTURAS}/{nombre}.png";
            File.WriteAllBytes(ruta, bytes);
            AssetDatabase.ImportAsset(ruta, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
        }

        private static MaterialesJardin GenerarMaterialesYTexturas()
        {
            // 1. Textura de Arena Zen rastrillada (patrón de ondas sinusoidales de rastrillo zen)
            Texture2D texArena = GenerarTexturaProcedural(512, 512, (x, y, u, v) =>
            {
                float ondaRastrillo = Mathf.Sin((u * 40f + Mathf.Sin(v * 10f) * 1.5f) * Mathf.PI * 2f) * 0.12f;
                float ruidoGrano = (Mathf.PerlinNoise(u * 80f, v * 80f) - 0.5f) * 0.08f;
                float brillo = 0.88f + ondaRastrillo + ruidoGrano;
                return new Color(brillo * 0.95f, brillo * 0.88f, brillo * 0.74f);
            });
            Texture2D assetArena = GuardarTexturaPNG("Tex_ArenaZen", texArena);

            // 2. Textura de Madera de Cedro Rojo (para el Portal Torii)
            Texture2D texMaderaTorii = GenerarTexturaProcedural(512, 512, (x, y, u, v) =>
            {
                float fibra = Mathf.Sin(v * 60f + Mathf.PerlinNoise(u * 15f, v * 50f) * 8f) * 0.08f;
                float vetas = (Mathf.PerlinNoise(u * 4f, v * 30f) - 0.5f) * 0.15f;
                Color baseColor = new Color(0.68f, 0.20f, 0.12f);
                return baseColor + new Color(fibra + vetas, fibra * 0.5f, 0f);
            });
            Texture2D assetMaderaTorii = GuardarTexturaPNG("Tex_MaderaTorii", texMaderaTorii);

            // 3. Textura de Madera Oscura de Templo (Teca / Ébano)
            Texture2D texMaderaOscura = GenerarTexturaProcedural(512, 512, (x, y, u, v) =>
            {
                float fibra = Mathf.Sin(v * 80f + Mathf.PerlinNoise(u * 10f, v * 60f) * 6f) * 0.05f;
                float tono = 0.22f + fibra + (Mathf.PerlinNoise(u * 6f, v * 25f) - 0.5f) * 0.08f;
                return new Color(tono * 1.1f, tono * 0.85f, tono * 0.65f);
            });
            Texture2D assetMaderaOscura = GuardarTexturaPNG("Tex_MaderaOscura", texMaderaOscura);

            // 4. Textura de Piedra y Musgo (para linternas, rocas e islas zen)
            Texture2D texPiedraMusgo = GenerarTexturaProcedural(512, 512, (x, y, u, v) =>
            {
                float ruidoRoca = Mathf.PerlinNoise(u * 20f, v * 20f) * 0.6f + Mathf.PerlinNoise(u * 60f, v * 60f) * 0.4f;
                float mascaraMusgo = Mathf.PerlinNoise(u * 8f, v * 8f);
                Color colorRoca = Color.Lerp(new Color(0.38f, 0.40f, 0.39f), new Color(0.65f, 0.67f, 0.65f), ruidoRoca);
                Color colorMusgo = new Color(0.28f, 0.45f, 0.22f);
                return mascaraMusgo > 0.62f ? Color.Lerp(colorRoca, colorMusgo, (mascaraMusgo - 0.62f) * 2.5f) : colorRoca;
            });
            Texture2D assetPiedraMusgo = GuardarTexturaPNG("Tex_PiedraMusgo", texPiedraMusgo);

            // 5. Textura de Agua Zen (Estanque de Lotos)
            Texture2D texAgua = GenerarTexturaProcedural(512, 512, (x, y, u, v) =>
            {
                float distCentro = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f));
                float onda = Mathf.Sin(distCentro * 35f - Mathf.PerlinNoise(u * 10f, v * 10f) * 4f) * 0.12f;
                Color baseAgua = new Color(0.12f, 0.42f, 0.52f);
                Color reflejo = new Color(0.35f, 0.70f, 0.78f);
                return Color.Lerp(baseAgua, reflejo, onda + 0.15f);
            });
            Texture2D assetAgua = GuardarTexturaPNG("Tex_AguaZen", texAgua);

            // 6. Textura de Papel Washi / Shoji para Linternas Japonesas
            Texture2D texPapelShoji = GenerarTexturaProcedural(512, 512, (x, y, u, v) =>
            {
                int celdas = 8;
                float franjaU = (u * celdas) % 1f;
                float franjaV = (v * celdas) % 1f;
                bool esRejilla = (franjaU < 0.08f || franjaV < 0.08f);

                if (esRejilla)
                    return new Color(0.22f, 0.14f, 0.08f); // Rejilla de bambú oscuro
                else
                {
                    float fibraPapel = (Mathf.PerlinNoise(u * 100f, v * 100f) - 0.5f) * 0.06f;
                    return new Color(0.96f + fibraPapel, 0.93f + fibraPapel, 0.82f + fibraPapel);
                }
            });
            Texture2D assetPapelShoji = GuardarTexturaPNG("Tex_PapelShoji", texPapelShoji);

            // 7. Textura de Oro / Bronce para Campana y Altar
            Texture2D texOro = GenerarTexturaProcedural(256, 256, (x, y, u, v) =>
            {
                float cepillado = (Mathf.PerlinNoise(u * 50f, v * 4f) - 0.5f) * 0.1f;
                return new Color(0.92f + cepillado, 0.76f + cepillado, 0.28f + cepillado);
            });
            Texture2D assetOro = GuardarTexturaPNG("Tex_OroBronce", texOro);

            // 8. Textura de Hoja de Loto
            Texture2D texLoto = GenerarTexturaProcedural(256, 256, (x, y, u, v) =>
            {
                float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
                float angulo = Mathf.Atan2(v - 0.5f, u - 0.5f);
                float venas = Mathf.Abs(Mathf.Sin(angulo * 12f)) * 0.15f;
                Color verdeLoto = new Color(0.22f, 0.58f, 0.25f);
                return verdeLoto * (1.1f - d * 0.4f + venas);
            });
            Texture2D assetLoto = GuardarTexturaPNG("Tex_HojaLoto", texLoto);

            AssetDatabase.SaveAssets();

            // Creación de materiales vinculados con propiedades PBR y Tiling (mosaico)
            MaterialesJardin mats = new MaterialesJardin();
            mats.arenaZen = CrearOGuardarMaterial("Mat_ArenaZen", assetArena, Color.white, 0.05f, 0.1f, new Vector2(6f, 6f));
            mats.maderaTorii = CrearOGuardarMaterial("Mat_MaderaTorii", assetMaderaTorii, Color.white, 0.15f, 0.45f, new Vector2(1f, 3f));
            mats.maderaOscura = CrearOGuardarMaterial("Mat_MaderaOscura", assetMaderaOscura, Color.white, 0.2f, 0.4f, new Vector2(2f, 2f));
            mats.piedraMusgo = CrearOGuardarMaterial("Mat_PiedraMusgo", assetPiedraMusgo, Color.white, 0.05f, 0.25f, new Vector2(2f, 2f));
            mats.aguaZen = CrearOGuardarMaterial("Mat_AguaZen", assetAgua, Color.white, 0.1f, 0.95f, new Vector2(3f, 3f));
            mats.papelFarola = CrearOGuardarMaterial("Mat_PapelFarola", assetPapelShoji, Color.white, 0.0f, 0.2f, new Vector2(1f, 1f), true, new Color(1.2f, 0.85f, 0.45f));
            mats.oroBronce = CrearOGuardarMaterial("Mat_OroBronce", assetOro, Color.white, 0.85f, 0.75f, new Vector2(2f, 2f));
            mats.hojasLoto = CrearOGuardarMaterial("Mat_HojasLoto", assetLoto, Color.white, 0.05f, 0.5f, new Vector2(1f, 1f));
            mats.cristalZen = CrearOGuardarMaterial("Mat_CristalZen", assetOro, new Color(0.6f, 0.9f, 1f), 0.3f, 0.95f, new Vector2(1f, 1f), true, new Color(0.4f, 0.8f, 1.2f));

            return mats;
        }

        private static Texture2D GenerarTexturaProcedural(int width, int height, Func<int, int, float, float, Color> calculadorPixel)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixeles = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    pixeles[y * width + x] = calculadorPixel(x, y, u, v);
                }
            }
            tex.SetPixels(pixeles);
            tex.Apply();
            return tex;
        }

        private static AudioClip GenerarAudioClipAmbiental()
        {
            string rutaArchivo = $"{RUTA_AUDIO}/Zen_Ambiental.wav";
            int sampleRate = 22050;
            float duracion = 4.0f; // 4 segundos en bucle armónico continuo
            int totalMuestras = Mathf.FloorToInt(sampleRate * duracion);

            using (FileStream stream = new FileStream(rutaArchivo, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Cabecera RIFF WAV estándar de 44 bytes
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + totalMuestras * 2);
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });

                // Sub-bloque FMT
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);                 // Tamaño sub-bloque
                writer.Write((short)1);           // Formato PCM
                writer.Write((short)1);           // Mono (1 canal)
                writer.Write(sampleRate);         // Frecuencia de muestreo
                writer.Write(sampleRate * 2);     // ByteRate (SampleRate * Canales * Bits / 8)
                writer.Write((short)2);           // BlockAlign
                writer.Write((short)16);          // 16 bits por muestra

                // Sub-bloque DATA
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(totalMuestras * 2);

                // Síntesis de tono meditativo de cuenco tibetano (frecuencias armónicas zen: 216Hz, 432Hz, 648Hz)
                float f1 = 216f;
                float f2 = 432f;
                float f3 = 648f;

                for (int i = 0; i < totalMuestras; i++)
                {
                    float t = (float)i / sampleRate;
                    // Ventana senoidal suave para un bucle transparente sin clics en los extremos
                    float ventanaLoop = Mathf.Sin((t / duracion) * Mathf.PI);

                    float tono = (Mathf.Sin(2f * Mathf.PI * f1 * t) * 0.45f
                                + Mathf.Sin(2f * Mathf.PI * f2 * t) * 0.25f
                                + Mathf.Sin(2f * Mathf.PI * f3 * t) * 0.10f) * ventanaLoop;

                    short muestra = (short)(Mathf.Clamp(tono, -1f, 1f) * 32767f);
                    writer.Write(muestra);
                }
            }

            AssetDatabase.ImportAsset(rutaArchivo, ImportAssetOptions.ForceUpdate);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(rutaArchivo);
        }

        #endregion

        #region Construcción 3D y Jerarquía (Escala 1:1 en Metros)

        private static GameObject CrearPrimitivo(PrimitiveType tipo, string nombre, Transform padre, Vector3 posLocal, Vector3 escalaLocal, Material mat, Quaternion? rotLocal = null)
        {
            GameObject obj = GameObject.CreatePrimitive(tipo);
            obj.name = nombre;
            obj.transform.SetParent(padre, false);
            obj.transform.localPosition = posLocal;
            obj.transform.localScale = escalaLocal;
            obj.transform.localRotation = rotLocal ?? Quaternion.identity;

            if (mat != null)
            {
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null) rend.sharedMaterial = mat;
            }

            return obj;
        }

        private static void ConstruirSueloYLimites(Transform raiz, MaterialesJardin mats)
        {
            GameObject grupoSuelo = new GameObject("Entorno_Suelo_Zen");
            grupoSuelo.transform.SetParent(raiz, false);

            // 1. Base sólida del suelo (Cubo)
            CrearPrimitivo(PrimitiveType.Cube, "Base_Suelo_Solida", grupoSuelo.transform,
                new Vector3(0f, -0.1f, 0f), new Vector3(24f, 0.2f, 24f), mats.maderaOscura);

            // 2. Superficie de Arena Zen rastrillada usando Plano primitivo (24m x 24m; escala de Plano en Unity: 1 unidad = 10m)
            CrearPrimitivo(PrimitiveType.Plane, "Superficie_Arena_Zen_Plano", grupoSuelo.transform,
                new Vector3(0f, 0.005f, 0f), new Vector3(2.4f, 1f, 2.4f), mats.arenaZen);

            // 3. Marco perimetral de madera oscura alrededor del jardín zen (Cubos)
            CrearPrimitivo(PrimitiveType.Cube, "Borde_Norte", grupoSuelo.transform,
                new Vector3(0f, 0.1f, 12f), new Vector3(24.8f, 0.3f, 0.4f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Borde_Sur", grupoSuelo.transform,
                new Vector3(0f, 0.1f, -12f), new Vector3(24.8f, 0.3f, 0.4f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Borde_Este", grupoSuelo.transform,
                new Vector3(12f, 0.1f, 0f), new Vector3(0.4f, 0.3f, 24.8f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Borde_Oeste", grupoSuelo.transform,
                new Vector3(-12f, 0.1f, 0f), new Vector3(0.4f, 0.3f, 24.8f), mats.maderaOscura);

            // 4. Muro zen de fondo (pared contemplativa de bambú / madera)
            CrearPrimitivo(PrimitiveType.Cube, "Muro_Fondo_Zen", grupoSuelo.transform,
                new Vector3(0f, 1.5f, 11.8f), new Vector3(24f, 3.0f, 0.25f), mats.maderaOscura);
        }

        private static void ConstruirPortalTorii(Transform raiz, MaterialesJardin mats)
        {
            // Objeto compuesto: Portal Torii tradicional japonés bajo GameObject vacío para manipulación conjunta
            GameObject torii = new GameObject("Portal_Torii");
            torii.transform.SetParent(raiz, false);
            torii.transform.localPosition = new Vector3(0f, 0f, -3.5f);

            // 1. Bases de piedra (Kamebara / Kamaboko)
            CrearPrimitivo(PrimitiveType.Cylinder, "Base_Piedra_Izq", torii.transform,
                new Vector3(-1.8f, 0.2f, 0f), new Vector3(0.6f, 0.2f, 0.6f), mats.piedraMusgo);
            CrearPrimitivo(PrimitiveType.Cylinder, "Base_Piedra_Der", torii.transform,
                new Vector3(1.8f, 0.2f, 0f), new Vector3(0.6f, 0.2f, 0.6f), mats.piedraMusgo);

            // 2. Columnas verticales principales (Hashira) de 3.6m de altura
            CrearPrimitivo(PrimitiveType.Cylinder, "Columna_Hashira_Izq", torii.transform,
                new Vector3(-1.8f, 2.0f, 0f), new Vector3(0.36f, 1.8f, 0.36f), mats.maderaTorii);
            CrearPrimitivo(PrimitiveType.Cylinder, "Columna_Hashira_Der", torii.transform,
                new Vector3(1.8f, 2.0f, 0f), new Vector3(0.36f, 1.8f, 0.36f), mats.maderaTorii);

            // 3. Dintel principal superior (Kasagi / Shimaki) con leve inclinación estética
            CrearPrimitivo(PrimitiveType.Cube, "Dintel_Kasagi_Superior", torii.transform,
                new Vector3(0f, 3.95f, 0f), new Vector3(4.5f, 0.3f, 0.45f), mats.maderaTorii);

            // 4. Travesaño secundario inferior (Nuki)
            CrearPrimitivo(PrimitiveType.Cube, "Travesano_Nuki_Inferior", torii.transform,
                new Vector3(0f, 3.2f, 0f), new Vector3(4.0f, 0.18f, 0.25f), mats.maderaTorii);

            // 5. Placa sagrada central (Gakuzuka)
            CrearPrimitivo(PrimitiveType.Cube, "Placa_Gakuzuka", torii.transform,
                new Vector3(0f, 3.55f, 0f), new Vector3(0.35f, 0.55f, 0.12f), mats.maderaOscura);
        }

        private static void ConstruirTemploPagoda(Transform raiz, MaterialesJardin mats)
        {
            // Objeto compuesto: Templo / Pagoda Meditativa bajo GameObject vacío
            GameObject pagoda = new GameObject("Templo_Pagoda_Zen");
            pagoda.transform.SetParent(raiz, false);
            pagoda.transform.localPosition = new Vector3(0f, 0f, 5.5f);

            // 1. Plataforma escalonada
            CrearPrimitivo(PrimitiveType.Cube, "Base_Piedra_Inferior", pagoda.transform,
                new Vector3(0f, 0.15f, 0f), new Vector3(7.2f, 0.3f, 7.2f), mats.piedraMusgo);
            CrearPrimitivo(PrimitiveType.Cube, "Terraza_Madera_Superior", pagoda.transform,
                new Vector3(0f, 0.4f, 0f), new Vector3(6.2f, 0.2f, 6.2f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Plane, "Tatami_Meditacion_Plano", pagoda.transform,
                new Vector3(0f, 0.505f, 0f), new Vector3(0.35f, 1f, 0.35f), mats.papelFarola);
            CrearPrimitivo(PrimitiveType.Cube, "Escalon_Acceso", pagoda.transform,
                new Vector3(0f, 0.15f, -3.3f), new Vector3(2.5f, 0.3f, 0.8f), mats.piedraMusgo);

            // 2. Cuatro columnas del templo (3.0m de altura)
            float offsetCol = 2.4f;
            Vector3[] posicionesCols = new Vector3[]
            {
                new Vector3(-offsetCol, 2.0f, -offsetCol),
                new Vector3(offsetCol, 2.0f, -offsetCol),
                new Vector3(-offsetCol, 2.0f, offsetCol),
                new Vector3(offsetCol, 2.0f, offsetCol)
            };

            for (int i = 0; i < posicionesCols.Length; i++)
            {
                CrearPrimitivo(PrimitiveType.Cylinder, $"Columna_Templo_{i + 1}", pagoda.transform,
                    posicionesCols[i], new Vector3(0.32f, 1.5f, 0.32f), mats.maderaOscura);
            }

            // 3. Estructura de vigas superiores
            CrearPrimitivo(PrimitiveType.Cube, "Viga_Norte", pagoda.transform,
                new Vector3(0f, 3.55f, offsetCol), new Vector3(5.1f, 0.22f, 0.25f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Viga_Sur", pagoda.transform,
                new Vector3(0f, 3.55f, -offsetCol), new Vector3(5.1f, 0.22f, 0.25f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Viga_Este", pagoda.transform,
                new Vector3(offsetCol, 3.55f, 0f), new Vector3(0.25f, 0.22f, 5.1f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Viga_Oeste", pagoda.transform,
                new Vector3(-offsetCol, 3.55f, 0f), new Vector3(0.25f, 0.22f, 5.1f), mats.maderaOscura);

            // 4. Techo de pagoda voladizo de múltiples niveles
            CrearPrimitivo(PrimitiveType.Cube, "Techo_Nivel1_Alero", pagoda.transform,
                new Vector3(0f, 3.8f, 0f), new Vector3(7.0f, 0.22f, 7.0f), mats.maderaTorii);
            CrearPrimitivo(PrimitiveType.Cube, "Techo_Nivel2_Medio", pagoda.transform,
                new Vector3(0f, 4.25f, 0f), new Vector3(5.2f, 0.3f, 5.2f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Techo_Nivel3_Cupula", pagoda.transform,
                new Vector3(0f, 4.75f, 0f), new Vector3(3.6f, 0.35f, 3.6f), mats.maderaTorii);

            // 5. Aguja zen y remate sagrado (Sōrin)
            CrearPrimitivo(PrimitiveType.Cylinder, "Sorin_Aguja", pagoda.transform,
                new Vector3(0f, 5.35f, 0f), new Vector3(0.14f, 0.45f, 0.14f), mats.oroBronce);
            CrearPrimitivo(PrimitiveType.Sphere, "Sorin_Remate_Esfera", pagoda.transform,
                new Vector3(0f, 5.9f, 0f), new Vector3(0.35f, 0.35f, 0.35f), mats.oroBronce);

            // 6. Campana zen de bronce colgante (Bonshō)
            CrearPrimitivo(PrimitiveType.Cylinder, "Campana_Cadena", pagoda.transform,
                new Vector3(0f, 3.35f, 0f), new Vector3(0.06f, 0.25f, 0.06f), mats.oroBronce);
            CrearPrimitivo(PrimitiveType.Cylinder, "Campana_Cuerpo", pagoda.transform,
                new Vector3(0f, 2.85f, 0f), new Vector3(0.55f, 0.4f, 0.55f), mats.oroBronce);
            CrearPrimitivo(PrimitiveType.Sphere, "Campana_Cupula", pagoda.transform,
                new Vector3(0f, 3.25f, 0f), new Vector3(0.55f, 0.3f, 0.55f), mats.oroBronce);
        }

        private static void ConstruirAltarMeditativo(Transform raiz, MaterialesJardin mats)
        {
            // Objeto compuesto: Altar de meditación bajo GameObject vacío
            GameObject altar = new GameObject("Altar_Meditativo");
            altar.transform.SetParent(raiz, false);
            altar.transform.localPosition = new Vector3(0f, 0.5f, 4.2f); // Elevado sobre la terraza de la pagoda

            // 1. Tablero superior de la mesa del altar
            CrearPrimitivo(PrimitiveType.Cube, "Mesa_Tablero", altar.transform,
                new Vector3(0f, 0.85f, 0f), new Vector3(1.8f, 0.1f, 0.9f), mats.maderaOscura);

            // 2. Cuatro patas del altar
            float px = 0.75f;
            float pz = 0.32f;
            CrearPrimitivo(PrimitiveType.Cube, "Pata_1", altar.transform, new Vector3(-px, 0.425f, -pz), new Vector3(0.12f, 0.75f, 0.12f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Pata_2", altar.transform, new Vector3(px, 0.425f, -pz), new Vector3(0.12f, 0.75f, 0.12f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Pata_3", altar.transform, new Vector3(-px, 0.425f, pz), new Vector3(0.12f, 0.75f, 0.12f), mats.maderaOscura);
            CrearPrimitivo(PrimitiveType.Cube, "Pata_4", altar.transform, new Vector3(px, 0.425f, pz), new Vector3(0.12f, 0.75f, 0.12f), mats.maderaOscura);

            // 3. Quemador de incienso zen (Kōrō)
            CrearPrimitivo(PrimitiveType.Cylinder, "Quemador_Base", altar.transform,
                new Vector3(0f, 0.95f, 0f), new Vector3(0.3f, 0.08f, 0.3f), mats.oroBronce);
            CrearPrimitivo(PrimitiveType.Sphere, "Quemador_Copa", altar.transform,
                new Vector3(0f, 1.05f, 0f), new Vector3(0.24f, 0.16f, 0.24f), mats.oroBronce);

            // 4. Elemento decorativo flotante: Orbe / Cristal Zen con Rotador Suave
            GameObject orbeFlotante = CrearPrimitivo(PrimitiveType.Sphere, "Orbe_Cristal_Flotante", altar.transform,
                new Vector3(0f, 1.45f, 0f), new Vector3(0.32f, 0.32f, 0.32f), mats.cristalZen);

            // Adjuntar script de animación RotadorSuave (Extra)
            Type tipoRotador = Type.GetType("ZenGardenVR.RotadorSuave, Assembly-CSharp");
            if (tipoRotador != null)
            {
                orbeFlotante.AddComponent(tipoRotador);
            }
            else
            {
                // Fallback dinámico si no estuviera compilado aún
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = assembly.GetType("ZenGardenVR.RotadorSuave");
                    if (t != null)
                    {
                        orbeFlotante.AddComponent(t);
                        break;
                    }
                }
            }

            // 5. Point Light ornamental en el altar (resplandor dorado tenue)
            GameObject luzAltar = new GameObject("Luz_Altar_Ornamental");
            luzAltar.transform.SetParent(altar.transform, false);
            luzAltar.transform.localPosition = new Vector3(0f, 1.45f, 0f);

            Light lightComponent = luzAltar.AddComponent<Light>();
            lightComponent.type = LightType.Point;
            lightComponent.color = new Color(1.0f, 0.78f, 0.42f);
            lightComponent.intensity = 1.8f;
            lightComponent.range = 3.5f;
            lightComponent.shadows = LightShadows.Soft;
        }

        private static void ConstruirFarolasToro(Transform raiz, MaterialesJardin mats)
        {
            // Dos farolas de piedra tradicionales (Tōrō) a los lados del camino zen
            ConstruirLinternaIndividual(raiz, mats, "Linterna_Toro_Izquierda", new Vector3(-3.2f, 0f, -1.0f));
            ConstruirLinternaIndividual(raiz, mats, "Linterna_Toro_Derecha", new Vector3(3.2f, 0f, -1.0f));
        }

        private static void ConstruirLinternaIndividual(Transform raiz, MaterialesJardin mats, string nombre, Vector3 posicion)
        {
            // Objeto compuesto con jerarquía estricta bajo GameObject vacío
            GameObject linterna = new GameObject(nombre);
            linterna.transform.SetParent(raiz, false);
            linterna.transform.localPosition = posicion;

            // 1. Base inferior (Kiso)
            CrearPrimitivo(PrimitiveType.Cylinder, "Base_Kiso", linterna.transform,
                new Vector3(0f, 0.15f, 0f), new Vector3(0.7f, 0.15f, 0.7f), mats.piedraMusgo);

            // 2. Fuste o pilar central (Sao) de 0.9m de altura
            CrearPrimitivo(PrimitiveType.Cylinder, "Pilar_Sao", linterna.transform,
                new Vector3(0f, 0.7f, 0f), new Vector3(0.32f, 0.45f, 0.32f), mats.piedraMusgo);

            // 3. Plataforma intermedia (Chūdai)
            CrearPrimitivo(PrimitiveType.Cube, "Plataforma_Chudai", linterna.transform,
                new Vector3(0f, 1.2f, 0f), new Vector3(0.75f, 0.12f, 0.75f), mats.piedraMusgo);

            // 4. Cámara de luz (Hibukuro) con papel shoji translúcido y postes de esquina
            CrearPrimitivo(PrimitiveType.Cube, "Camara_Papel_Luz", linterna.transform,
                new Vector3(0f, 1.55f, 0f), new Vector3(0.48f, 0.45f, 0.48f), mats.papelFarola);

            float cp = 0.22f;
            CrearPrimitivo(PrimitiveType.Cube, "Poste_1", linterna.transform, new Vector3(-cp, 1.55f, -cp), new Vector3(0.08f, 0.45f, 0.08f), mats.piedraMusgo);
            CrearPrimitivo(PrimitiveType.Cube, "Poste_2", linterna.transform, new Vector3(cp, 1.55f, -cp), new Vector3(0.08f, 0.45f, 0.08f), mats.piedraMusgo);
            CrearPrimitivo(PrimitiveType.Cube, "Poste_3", linterna.transform, new Vector3(-cp, 1.55f, cp), new Vector3(0.08f, 0.45f, 0.08f), mats.piedraMusgo);
            CrearPrimitivo(PrimitiveType.Cube, "Poste_4", linterna.transform, new Vector3(cp, 1.55f, cp), new Vector3(0.08f, 0.45f, 0.08f), mats.piedraMusgo);

            // 5. Techo protector paraguas (Kasa)
            CrearPrimitivo(PrimitiveType.Cube, "Techo_Kasa", linterna.transform,
                new Vector3(0f, 1.9f, 0f), new Vector3(0.95f, 0.15f, 0.95f), mats.piedraMusgo);

            // 6. Remate sagrado superior (Hōju)
            CrearPrimitivo(PrimitiveType.Sphere, "Remate_Hoju", linterna.transform,
                new Vector3(0f, 2.15f, 0f), new Vector3(0.28f, 0.28f, 0.28f), mats.piedraMusgo);

            // 7. Point Light ornamental dentro de la linterna
            GameObject luzLinterna = new GameObject("Luz_Linterna_Point");
            luzLinterna.transform.SetParent(linterna.transform, false);
            luzLinterna.transform.localPosition = new Vector3(0f, 1.55f, 0f);

            Light lightComponent = luzLinterna.AddComponent<Light>();
            lightComponent.type = LightType.Point;
            lightComponent.color = new Color(1.0f, 0.65f, 0.28f); // Fuego cálido de vela
            lightComponent.intensity = 2.2f;
            lightComponent.range = 4.5f;
            lightComponent.shadows = LightShadows.Soft;
        }

        private static void ConstruirEstanqueLoto(Transform raiz, MaterialesJardin mats)
        {
            // Objeto compuesto: Estanque de agua zen con loto bajo GameObject vacío
            GameObject estanque = new GameObject("Estanque_Loto_Zen");
            estanque.transform.SetParent(raiz, false);
            estanque.transform.localPosition = new Vector3(-5.5f, 0f, 1.5f);

            // 1. Superficie de agua circular (Cilindro aplanado de 5m de diámetro)
            CrearPrimitivo(PrimitiveType.Cylinder, "Superficie_Agua", estanque.transform,
                new Vector3(0f, 0.04f, 0f), new Vector3(5.0f, 0.04f, 5.0f), mats.aguaZen);

            // 2. Borde perimetral de rocas redondeadas
            int totalRocas = 14;
            float radioEstanque = 2.5f;
            for (int i = 0; i < totalRocas; i++)
            {
                float angulo = (i / (float)totalRocas) * Mathf.PI * 2f;
                float x = Mathf.Cos(angulo) * radioEstanque;
                float z = Mathf.Sin(angulo) * radioEstanque;
                float variacionEscala = 0.45f + (i % 3) * 0.12f;

                CrearPrimitivo(PrimitiveType.Sphere, $"Roca_Borde_{i + 1}", estanque.transform,
                    new Vector3(x, 0.08f, z), new Vector3(variacionEscala, 0.25f, variacionEscala), mats.piedraMusgo,
                    Quaternion.Euler(0f, i * 25f, 0f));
            }

            // 3. Hojas de loto flotantes
            CrearPrimitivo(PrimitiveType.Cylinder, "Hoja_Loto_1", estanque.transform,
                new Vector3(0.8f, 0.065f, 0.6f), new Vector3(0.7f, 0.015f, 0.7f), mats.hojasLoto);
            CrearPrimitivo(PrimitiveType.Cylinder, "Hoja_Loto_2", estanque.transform,
                new Vector3(-0.7f, 0.065f, -0.8f), new Vector3(0.6f, 0.015f, 0.6f), mats.hojasLoto);

            // 4. Flor de loto sagrada
            CrearPrimitivo(PrimitiveType.Sphere, "Flor_Loto_Sagrada", estanque.transform,
                new Vector3(0.8f, 0.14f, 0.6f), new Vector3(0.24f, 0.26f, 0.24f), mats.cristalZen);
        }

        private static void ConstruirIslaRocasZen(Transform raiz, MaterialesJardin mats)
        {
            // Objeto compuesto: Isla de rocas zen meditativas (Karesansui Iwakura)
            GameObject islaRocas = new GameObject("Isla_Rocas_Karesansui");
            islaRocas.transform.SetParent(raiz, false);
            islaRocas.transform.localPosition = new Vector3(5.5f, 0f, 1.5f);

            // 1. Montículo de musgo base
            CrearPrimitivo(PrimitiveType.Sphere, "Monticulo_Musgo", islaRocas.transform,
                new Vector3(0f, 0.08f, 0f), new Vector3(3.8f, 0.25f, 3.2f), mats.hojasLoto);

            // 2. Tríada de piedras zen clásicas con escala no uniforme y rotaciones naturales
            // Roca central Padre (alta, esbelta)
            CrearPrimitivo(PrimitiveType.Sphere, "Roca_Central_Padre", islaRocas.transform,
                new Vector3(0f, 0.8f, 0f), new Vector3(1.1f, 1.7f, 0.95f), mats.piedraMusgo,
                Quaternion.Euler(12f, 25f, -6f));

            // Roca Guardián
            CrearPrimitivo(PrimitiveType.Sphere, "Roca_Guardian_Izq", islaRocas.transform,
                new Vector3(-0.95f, 0.45f, 0.2f), new Vector3(0.8f, 0.95f, 0.75f), mats.piedraMusgo,
                Quaternion.Euler(-8f, 55f, 15f));

            // Roca Hijo
            CrearPrimitivo(PrimitiveType.Sphere, "Roca_Hijo_Der", islaRocas.transform,
                new Vector3(0.9f, 0.35f, -0.3f), new Vector3(0.65f, 0.7f, 0.6f), mats.piedraMusgo,
                Quaternion.Euler(16f, -40f, 8f));
        }

        private static void ConstruirCaminoPiedras(Transform raiz, MaterialesJardin mats)
        {
            // Objeto compuesto: Camino de piedras de paso (Tobi-ishi) desde el punto de vista del usuario
            GameObject camino = new GameObject("Camino_Piedras_Paso");
            camino.transform.SetParent(raiz, false);

            float[] posicionesZ = new float[] { -5.5f, -4.5f, -3.5f, -2.5f, -1.5f, -0.5f, 0.6f };
            float[] oscilacionesX = new float[] { 0f, -0.15f, 0.12f, -0.1f, 0.15f, -0.08f, 0.05f };

            for (int i = 0; i < posicionesZ.Length; i++)
            {
                CrearPrimitivo(PrimitiveType.Cylinder, $"Piedra_Paso_{i + 1}", camino.transform,
                    new Vector3(oscilacionesX[i], 0.035f, posicionesZ[i]),
                    new Vector3(0.65f, 0.04f, 0.6f),
                    mats.piedraMusgo,
                    Quaternion.Euler(0f, i * 40f, 0f));
            }
        }

        private static void ConstruirIluminacion(Transform raiz)
        {
            // Contenedor de luces
            GameObject grupoLuz = new GameObject("Iluminacion_Jardin");
            grupoLuz.transform.SetParent(raiz, false);

            // Luz Direccional (Sol zen con sombras suaves)
            // Eliminar luces direccionales existentes para evitar sobreexposición
            Light[] lucesExistentes = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var l in lucesExistentes)
            {
                if (l.type == LightType.Directional && l.gameObject.name != "Sol_Meditativo_VR")
                {
                    Undo.DestroyObjectImmediate(l.gameObject);
                }
            }

            GameObject solObj = new GameObject("Sol_Meditativo_VR");
            solObj.transform.SetParent(grupoLuz.transform, false);
            solObj.transform.localRotation = Quaternion.Euler(46f, -32f, 0f);

            Light sol = solObj.AddComponent<Light>();
            sol.type = LightType.Directional;
            sol.color = new Color(1.0f, 0.96f, 0.88f); // Luz solar matutina serena
            sol.intensity = 1.25f;
            sol.shadows = LightShadows.Soft;
            sol.shadowStrength = 0.85f;
        }

        private static void ConstruirEmisorAudio(Transform raiz, AudioClip clipAmbiental)
        {
            GameObject audioObj = new GameObject("Emisor_Audio_Ambiental");
            audioObj.transform.SetParent(raiz, false);
            audioObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            AudioSource audioSource = audioObj.AddComponent<AudioSource>();
            audioSource.clip = clipAmbiental;
            audioSource.playOnAwake = true;
            audioSource.loop = true;
            audioSource.spatialBlend = 0.0f; // 2D ambiental relajante
            audioSource.volume = 0.35f;
        }

        private static void ConfigurarCamaraVR()
        {
            // Buscar o crear rig de RV
            GameObject vrRig = GameObject.Find("[VR Rig] Jugador");
            if (vrRig == null)
            {
                vrRig = new GameObject("[VR Rig] Jugador");
                Undo.RegisterCreatedObjectUndo(vrRig, "Crear VR Rig");
            }
            vrRig.transform.position = new Vector3(0f, 0f, -6.5f); // Punto de inicio frente al jardín
            vrRig.transform.rotation = Quaternion.identity;

            // Buscar la cámara principal existente o crear una nueva
            Camera cam = Camera.main;
            GameObject camObj = null;

            if (cam != null)
            {
                camObj = cam.gameObject;
            }
            else
            {
                camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
            }

            camObj.name = "Main Camera";
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(vrRig.transform, false);

            // Escala 1:1 en metros: Altura realista de ojos humanos = 1.65m
            camObj.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            camObj.transform.localRotation = Quaternion.identity;
            cam.nearClipPlane = 0.05f; // Óptimo para RV (evita recortes de objetos cercanos)

            // Asegurar AudioListener
            if (camObj.GetComponent<AudioListener>() == null)
            {
                camObj.AddComponent<AudioListener>();
            }

            // Añadir TrackedPoseDriver para Head Tracking de RV
            // Compatible con Input System (Unity 6 / URP) y Legacy XR
            Type tipoTrackedPoseDriver = null;

            // 1. Probar vía Unity.InputSystem (paquete instalado en Unity 6)
            tipoTrackedPoseDriver = Type.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver, Unity.InputSystem");

            // 2. Probar vía UnityEngine.SpatialTracking (Legacy XR)
            if (tipoTrackedPoseDriver == null)
            {
                tipoTrackedPoseDriver = Type.GetType("UnityEngine.SpatialTracking.TrackedPoseDriver, UnityEngine.SpatialTracking");
            }

            // 3. Búsqueda profunda en todos los ensamblados cargados
            if (tipoTrackedPoseDriver == null)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = assembly.GetType("UnityEngine.InputSystem.XR.TrackedPoseDriver")
                          ?? assembly.GetType("UnityEngine.SpatialTracking.TrackedPoseDriver");
                    if (t != null)
                    {
                        tipoTrackedPoseDriver = t;
                        break;
                    }
                }
            }

            if (tipoTrackedPoseDriver != null)
            {
                Component tpdExistente = camObj.GetComponent(tipoTrackedPoseDriver);
                if (tpdExistente == null)
                {
                    Component tpd = camObj.AddComponent(tipoTrackedPoseDriver);

                    // Configurar seguimiento completo de Posición y Rotación vía reflexión
                    PropertyInfo trackingTypeProp = tipoTrackedPoseDriver.GetProperty("trackingType");
                    if (trackingTypeProp != null && trackingTypeProp.CanWrite)
                    {
                        try
                        {
                            object valorRotAndPos = Enum.Parse(trackingTypeProp.PropertyType, "RotationAndPosition");
                            trackingTypeProp.SetValue(tpd, valorRotAndPos, null);
                        }
                        catch { }
                    }
                    Debug.Log($"<color=#2196F3>[Generador VR]</color> Componente TrackedPoseDriver ({tipoTrackedPoseDriver.FullName}) configurado en Main Camera a 1.65m de altura.");
                }
            }
            else
            {
                Debug.LogWarning("[Generador VR] No se encontró el componente TrackedPoseDriver en los paquetes del proyecto. Recuerda tener activo 'Input System' o 'XR Legacy Helpers'.");
            }
        }

        #endregion
    }
}
