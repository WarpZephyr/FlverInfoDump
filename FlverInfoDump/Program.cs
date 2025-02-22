using SoulsFormats;
using System;
using System.IO;

namespace FlverInfoDump
{
    internal class Program
    {
        const string ScopeValue = "  ";
        const string Extension = ".info.txt";
        static bool HadErrors;
        static bool HadWarnings;
        static bool WarnOnUnrecognizedTypes;

        static void Main(string[] args)
        {
            HadErrors = false;
            foreach (string arg in args)
            {
                try
                {
                    if (Directory.Exists(arg))
                    {
                        WarnOnUnrecognizedTypes = false;
                        ProcessFolder(arg);
                        WarnOnUnrecognizedTypes = true;
                    }
                    else if (File.Exists(arg))
                    {
                        Process(arg);
                    }
                    else
                    {
                        Error($"Could not find file or folder named: {arg}");
                    }
                }
                catch (Exception ex)
                {
                    Error($"A file failed processing due to an error:\nFile: {arg}\nError:\n{ex}");
                }
            }

            Console.WriteLine($"Finished.");
            if (HadErrors || HadWarnings)
            {
                Console.ReadKey();
            }
        }

        #region Process

        static void ProcessFolder(string folder)
        {
            bool found = false;
            foreach (string path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
            {
                found |= true;
                Process(path);
            }

            if (!found)
            {
                Warn($"No files found in folder: {folder}");
            }
        }

        static void Process(string file)
        {
            FileStream stream;
            StreamWriter sw;
            ScopeWriter tw;
            void InitializeWriter()
            {
                stream = File.OpenWrite(file + Extension);
                sw = new StreamWriter(stream);
                tw = new ScopeWriter(sw);
            }

            void DisposeWriter()
            {
                sw.Dispose();
                tw.Dispose();
                stream.Dispose();
            }

            if (MDL4.IsRead(file, out MDL4 mdl4))
            {
                InitializeWriter();
                ProcessMDL4(tw, mdl4);
            }
            else if (SMD4.IsRead(file, out SMD4 smd4))
            {
                InitializeWriter();
                ProcessSMD4(tw, smd4);
            }
            else if (FLVER0.IsRead(file, out FLVER0 flver0))
            {
                InitializeWriter();
                ProcessFLVER0(tw, flver0);
            }
            else if (FLVER2.IsRead(file, out FLVER2 flver2))
            {
                InitializeWriter();
                ProcessFLVER2(tw, flver2);
            }
            else if (file.EndsWith(Extension))
            {
                if (WarnOnUnrecognizedTypes)
                {
                    Warn($"File appears to be a written info txt: {file}");
                }
                return;
            }
            else
            {
                if (WarnOnUnrecognizedTypes)
                {
                    Error($"Unrecognized file type: {file}");
                }
                return;
            }

            DisposeWriter();
        }

        #endregion

        #region Process MDL4

        static void ProcessMDL4(ScopeWriter tw, MDL4 model)
        {
            tw.StartBuffering();
            tw.WriteLine($"Type: {nameof(MDL4)}");
            tw.ScopeTitleLine("Header:", ScopeValue);
            tw.WriteLine($"Version: 0x{model.Header.Version:X}");
            tw.WriteLine($"BoundingBoxMin: {model.Header.BoundingBoxMin}");
            tw.WriteLine($"BoundingBoxMax: {model.Header.BoundingBoxMax}");
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Materials:", ScopeValue);
            for (int i = 0; i < model.Materials.Count; i++)
            {
                var material = model.Materials[i];
                tw.ScopeTitleLine($"Material: {material.Name}", ScopeValue);
                tw.WriteLine($"Shader: {material.Shader}");
                tw.WriteLine($"Unk3C: {material.Unk3C}");
                tw.WriteLine($"Unk3D: {material.Unk3D}");
                tw.WriteLine($"Unk3E: {material.Unk3E}");
                tw.ScopeTitleLine($"Parameters:", ScopeValue);
                foreach (var parameter in material.Params)
                {
                    tw.ScopeTitleLine($"Parameter: {parameter.Name}", ScopeValue);
                    tw.WriteLine($"Type: {parameter.Type}");
                    tw.WriteLine($"Value: {Mdl4ParamValueToString(parameter)}");
                    tw.PopScope();
                }
                tw.PopScope();
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Dummies:", ScopeValue);
            for (int i = 0; i < model.Dummies.Count; i++)
            {
                var dummy = model.Dummies[i];
                tw.ScopeTitleLine($"Dummy: {i}", ScopeValue);
                tw.WriteLine($"Position: {dummy.Position}");
                tw.WriteLine($"Forward: {dummy.Forward}");
                tw.WriteLine($"Color: {dummy.Color}");
                tw.WriteLine($"ReferenceID: {dummy.ReferenceID}");
                tw.WriteLine($"ParentBoneIndex: {dummy.ParentBoneIndex}");
                tw.WriteLine($"AttachBoneIndex: {dummy.AttachBoneIndex}");
                tw.WriteLine($"Unk22: {dummy.Unk22}");
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Bones:", ScopeValue);
            for (int i = 0; i < model.Nodes.Count; i++)
            {
                var bone = model.Nodes[i];
                tw.ScopeTitleLine($"Bone: {bone.Name}", ScopeValue);
                tw.WriteLine($"Translation: {bone.Translation}");
                tw.WriteLine($"Rotation: {bone.Rotation}");
                tw.WriteLine($"Scale: {bone.Scale}");
                tw.WriteLine($"BoundingBoxMin: {bone.BoundingBoxMin}");
                tw.WriteLine($"BoundingBoxMax: {bone.BoundingBoxMax}");
                tw.WriteLine($"Index: {i}");
                tw.WriteLine($"Parent Index: {bone.ParentIndex}");
                tw.WriteLine($"Previous Sibling Index: {bone.PreviousSiblingIndex}");
                tw.WriteLine($"Next Sibling Index: {bone.NextSiblingIndex}");
                tw.WriteLine($"First Child Index: {bone.FirstChildIndex}");
                tw.WriteLine($"UnkIndices: [{string.Join(",", bone.UnkIndices)}]");
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Meshes:", ScopeValue);
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                tw.ScopeTitleLine($"Mesh: {i}", ScopeValue);
                tw.WriteLine($"MaterialIndex: {mesh.MaterialIndex}");
                tw.WriteLine($"VertexFormat: {mesh.VertexFormat}");
                tw.WriteLine($"Unk02: {mesh.Unk02}");
                tw.WriteLine($"Unk03: {mesh.Unk03}");
                tw.WriteLine($"Unk08: {mesh.Unk08}");
                tw.WriteLine($"BoneIndices: [{string.Join(",", mesh.BoneIndices)}]");
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();
        }

        #endregion

        #region Process SMD4

        static void ProcessSMD4(ScopeWriter tw, SMD4 model)
        {
            tw.StartBuffering();
            tw.WriteLine($"Type: {nameof(SMD4)}");
            tw.WriteLine("Kind: Shadow Mesh");
            tw.ScopeTitleLine("Header:", ScopeValue);
            tw.WriteLine($"Version: 0x{model.Header.Version:X}");
            tw.WriteLine($"BoundingBoxMin: {model.Header.BoundingBoxMin}");
            tw.WriteLine($"BoundingBoxMax: {model.Header.BoundingBoxMax}");
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Bones:", ScopeValue);
            for (int i = 0; i < model.Nodes.Count; i++)
            {
                var bone = model.Nodes[i];
                tw.ScopeTitleLine($"Bone: {bone.Name}", ScopeValue);
                tw.WriteLine($"Translation: {bone.Translation}");
                tw.WriteLine($"Rotation: {bone.Rotation}");
                tw.WriteLine($"Scale: {bone.Scale}");
                tw.WriteLine($"BoundingBoxMin: {bone.BoundingBoxMin}");
                tw.WriteLine($"BoundingBoxMax: {bone.BoundingBoxMax}");
                tw.WriteLine($"Index: {i}");
                tw.WriteLine($"Parent Index: {bone.ParentIndex}");
                tw.WriteLine($"Previous Sibling Index: {bone.PreviousSiblingIndex}");
                tw.WriteLine($"Next Sibling Index: {bone.NextSiblingIndex}");
                tw.WriteLine($"First Child Index: {bone.FirstChildIndex}");
                tw.WriteLine($"Unk64: {bone.Unk64}");
                tw.WriteLine($"Unk68: {bone.Unk68}");
                tw.WriteLine($"Unk6C: {bone.Unk6C}");
                tw.WriteLine($"Unk70: [{string.Join(",", bone.Unk70)}]");
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Meshes:", ScopeValue);
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                tw.ScopeTitleLine($"Mesh: {i}", ScopeValue);
                tw.WriteLine($"VertexFormat: {mesh.VertexFormat}");
                tw.WriteLine($"Unk01: {mesh.Unk01}");
                tw.WriteLine($"Unk02: {mesh.Unk02}");
                tw.WriteLine($"Unk03: {mesh.Unk03}");
                tw.WriteLine($"Unk06: {mesh.Unk06}");
                tw.WriteLine($"BoneIndices: [{string.Join(",", mesh.BoneIndices)}]");
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();
        }

        #endregion

        #region Process FLVER0

        static void ProcessFLVER0(ScopeWriter tw, FLVER0 model)
        {
            tw.StartBuffering();
            tw.ScopeTitleLine("Header:", ScopeValue);
            tw.WriteLine($"Type: {nameof(FLVER0)}");
            tw.WriteLine($"BigEndian: {model.Header.BigEndian}");
            tw.WriteLine($"Version: 0x{model.Header.Version:X}");
            tw.WriteLine($"BoundingBoxMin: {model.Header.BoundingBoxMin}");
            tw.WriteLine($"VertexIndexSize: {model.Header.VertexIndexSize}");
            tw.WriteLine($"Unicode: {model.Header.Unicode}");
            tw.WriteLine($"Unk4A: {model.Header.Unk4A}");
            tw.WriteLine($"Unk4B: {model.Header.Unk4B}");
            tw.WriteLine($"Unk4C: {model.Header.Unk4C}");
            tw.WriteLine($"Unk5C: {model.Header.Unk5C}");
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Materials:", ScopeValue);
            for (int i = 0; i < model.Materials.Count; i++)
            {
                var material = model.Materials[i];
                tw.ScopeTitleLine($"Material: {material.Name}", ScopeValue);
                tw.WriteLine($"MTD: {material.MTD}");
                tw.ScopeTitleLine($"Textures:", ScopeValue);
                for (int textureIndex = 0; textureIndex < material.Textures.Count; textureIndex++)
                {
                    var texture = material.Textures[textureIndex];
                    tw.ScopeTitleLine($"Texture: {textureIndex}", ScopeValue);
                    tw.WriteLine($"Type: {texture.Type}");
                    tw.WriteLine($"Path: {texture.Path}");
                    tw.PopScope();
                }
                tw.PopScope();

                tw.ScopeTitleLine($"Layouts:", ScopeValue);
                for (int layoutIndex = 0; layoutIndex < material.Layouts.Count; layoutIndex++)
                {
                    var layout = material.Layouts[layoutIndex];
                    tw.ScopeTitleLine($"Layout: {layoutIndex}", ScopeValue);
                    tw.WriteLine($"Size: {layout.Size}");
                    tw.ScopeTitleLine("LayoutMembers:", ScopeValue);
                    for (int layoutMemberIndex = 0; layoutMemberIndex < layout.Count; layoutMemberIndex++)
                    {
                        var layoutMember = layout[layoutMemberIndex];
                        PrintFlverLayoutMember(tw, layoutMember, layoutMemberIndex);
                    }
                    tw.PopScope();
                    tw.PopScope();
                }
                tw.PopScope();
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Dummies:", ScopeValue);
            for (int i = 0; i < model.Dummies.Count; i++)
            {
                var dummy = model.Dummies[i];
                PrintFlverDummy(tw, dummy, i);
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Bones:", ScopeValue);
            for (int i = 0; i < model.Nodes.Count; i++)
            {
                var bone = model.Nodes[i];
                PrintFlverNode(tw, bone, i);
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Meshes:", ScopeValue);
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                tw.ScopeTitleLine($"Mesh: {i}", ScopeValue);
                tw.WriteLine($"Dynamic: {mesh.Dynamic}");
                tw.WriteLine($"MaterialIndex: {mesh.MaterialIndex}");
                tw.WriteLine($"CullBackfaces: {mesh.CullBackfaces}");
                tw.WriteLine($"TriangleStrip: {mesh.TriangleStrip}");
                tw.WriteLine($"DefaultBoneIndex: {mesh.DefaultBoneIndex}");
                tw.WriteLine($"BoneIndices: [{string.Join(",", mesh.BoneIndices)}]");
                tw.WriteLine($"Unk46: {mesh.Unk46}");
                tw.WriteLine($"LayoutIndex: {mesh.LayoutIndex}");
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();
        }

        #endregion

        #region Process FLVER2

        static void ProcessFLVER2(ScopeWriter tw, FLVER2 model)
        {
            tw.StartBuffering();
            tw.WriteLine($"Type: {nameof(FLVER2)}");
            tw.ScopeTitleLine("Header:", ScopeValue);
            tw.WriteLine($"BigEndian: {model.Header.BigEndian}");
            tw.WriteLine($"Version: 0x{model.Header.Version:X}");
            tw.WriteLine($"BoundingBoxMin: {model.Header.BoundingBoxMin}");
            tw.WriteLine($"Unicode: {model.Header.Unicode}");
            tw.WriteLine($"Unk4A: {model.Header.Unk4A}");
            tw.WriteLine($"Unk4B: {model.Header.Unk4B}");
            tw.WriteLine($"Unk4C: {model.Header.Unk4C}");
            tw.WriteLine($"Unk5C: {model.Header.Unk5C}");
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Materials:", ScopeValue);
            for (int i = 0; i < model.Materials.Count; i++)
            {
                var material = model.Materials[i];
                tw.ScopeTitleLine($"Material: {material.Name}", ScopeValue);
                tw.WriteLine($"MTD: {material.MTD}");
                tw.ScopeTitleLine("Textures:", ScopeValue);
                for (int textureIndex = 0; textureIndex < material.Textures.Count; textureIndex++)
                {
                    var texture = material.Textures[textureIndex];
                    tw.ScopeTitleLine($"Texture: {textureIndex}", ScopeValue);
                    tw.WriteLine($"Type: {texture.Type}");
                    tw.WriteLine($"Path: {texture.Path}");
                    tw.PopScope();
                }
                tw.PopScope();
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine($"Layouts:", ScopeValue);
            for (int layoutIndex = 0; layoutIndex < model.BufferLayouts.Count; layoutIndex++)
            {
                var layout = model.BufferLayouts[layoutIndex];
                tw.ScopeTitleLine($"Layout: {layoutIndex}", ScopeValue);
                tw.WriteLine($"Size: {layout.Size}");
                tw.ScopeTitleLine("LayoutMembers:", ScopeValue);
                for (int layoutMemberIndex = 0; layoutMemberIndex < layout.Count; layoutMemberIndex++)
                {
                    var layoutMember = layout[layoutMemberIndex];
                    PrintFlverLayoutMember(tw, layoutMember, layoutMemberIndex);
                }
                tw.PopScope();
                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Dummies:", ScopeValue);
            for (int i = 0; i < model.Dummies.Count; i++)
            {
                var dummy = model.Dummies[i];
                PrintFlverDummy(tw, dummy, i);
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Bones:", ScopeValue);
            for (int i = 0; i < model.Nodes.Count; i++)
            {
                var bone = model.Nodes[i];
                PrintFlverNode(tw, bone, i);
            }
            tw.PopScope();
            tw.EndBuffering();

            tw.StartBuffering();
            tw.ScopeTitleLine("Meshes:", ScopeValue);
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[i];
                tw.ScopeTitleLine($"Mesh: {i}", ScopeValue);
                tw.WriteLine($"Dynamic: {mesh.Dynamic}");
                tw.WriteLine($"MaterialIndex: {mesh.MaterialIndex}");
                tw.WriteLine($"DefaultBoneIndex: {mesh.NodeIndex}");
                tw.WriteLine($"BoneIndices: [{string.Join(",", mesh.BoneIndices)}]");
                tw.ScopeTitleLine($"FaceSets:", ScopeValue);
                for (int facesetIndex = 0; facesetIndex < mesh.FaceSets.Count; facesetIndex++)
                {
                    var faceset = mesh.FaceSets[facesetIndex];
                    tw.ScopeTitleLine($"FaceSet: {facesetIndex}", ScopeValue);
                    tw.WriteLine($"Flags: {faceset.Flags}");
                    tw.WriteLine($"TriangleStrip: {faceset.TriangleStrip}");
                    tw.WriteLine($"CullBackfaces: {faceset.CullBackfaces}");
                    tw.WriteLine($"Unk06: {faceset.Unk06}");
                    tw.PopScope();
                }
                tw.PopScope();

                tw.ScopeTitleLine($"VertexBuffers:", ScopeValue);
                for (int vertexBufferIndex = 0; vertexBufferIndex < mesh.VertexBuffers.Count; vertexBufferIndex++)
                {
                    var vertexBuffer = mesh.VertexBuffers[vertexBufferIndex];
                    tw.ScopeTitleLine($"VertexBuffer: {vertexBufferIndex}", ScopeValue);
                    tw.WriteLine($"EdgeCompressed: {vertexBuffer.EdgeCompressed}");
                    tw.WriteLine($"BufferIndex: {vertexBuffer.BufferIndex}");
                    tw.WriteLine($"LayoutIndex: {vertexBuffer.LayoutIndex}");
                    tw.PopScope();
                }
                tw.PopScope();

                if (mesh.BoundingBox != null)
                {
                    var boundingBox = mesh.BoundingBox;
                    tw.ScopeTitleLine($"BoundingBoxes:", ScopeValue);
                    tw.WriteLine($"Min: {boundingBox.Min}");
                    tw.WriteLine($"Max: {boundingBox.Max}");
                    if (model.Header.Version >= 0x2001A)
                        tw.WriteLine($"Unk: {boundingBox.Unk}");
                    tw.PopScope();
                }

                tw.PopScope();
            }
            tw.PopScope();
            tw.EndBuffering();
        }

        #endregion

        #region Processing Helpers

        static string Mdl4ParamValueToString(MDL4.Material.MATParam param)
        {
            switch (param.Type)
            {
                case MDL4.Material.ParamType.Int:
                    return $"{param.Value as int?}";
                case MDL4.Material.ParamType.Float:
                    return $"{param.Value as float?}";
                case MDL4.Material.ParamType.Float4:
                    if (param.Value is float[] array)
                        return $"[{string.Join(",", array)}]";
                    else
                        return "null";
                case MDL4.Material.ParamType.String:
                    return $"{param.Value as string}";
                default:
                    throw new NotImplementedException($"Unknown {nameof(MDL4.Material.ParamType)}: {param.Type}");
            }
        }

        static void PrintFlverNode(ScopeWriter tw, FLVER.Node bone, int index)
        {
            tw.ScopeTitleLine($"Bone: {bone.Name}", ScopeValue);
            tw.WriteLine($"Translation: {bone.Translation}");
            tw.WriteLine($"Rotation: {bone.Rotation}");
            tw.WriteLine($"Scale: {bone.Scale}");
            tw.WriteLine($"BoundingBoxMin: {bone.BoundingBoxMin}");
            tw.WriteLine($"BoundingBoxMax: {bone.BoundingBoxMax}");
            tw.WriteLine($"Index: {index}");
            tw.WriteLine($"Parent Index: {bone.ParentIndex}");
            tw.WriteLine($"Previous Sibling Index: {bone.PreviousSiblingIndex}");
            tw.WriteLine($"Next Sibling Index: {bone.NextSiblingIndex}");
            tw.WriteLine($"First Child Index: {bone.FirstChildIndex}");
            tw.WriteLine($"Flags: [{bone.Flags}]");
            tw.PopScope();
        }

        static void PrintFlverDummy(ScopeWriter tw, FLVER.Dummy dummy, int index)
        {
            tw.ScopeTitleLine($"Dummy: {index}", ScopeValue);
            tw.WriteLine($"Position: {dummy.Position}");
            tw.WriteLine($"Forward: {dummy.Forward}");
            tw.WriteLine($"Upward: {dummy.Upward}");
            tw.WriteLine($"Color: {dummy.Color}");
            tw.WriteLine($"ReferenceID: {dummy.ReferenceID}");
            tw.WriteLine($"ParentBoneIndex: {dummy.ParentBoneIndex}");
            tw.WriteLine($"AttachBoneIndex: {dummy.AttachBoneIndex}");
            tw.WriteLine($"Flag1: {dummy.Flag1}");
            tw.WriteLine($"UseUpwardVector: {dummy.UseUpwardVector}");
            tw.WriteLine($"Unk30: {dummy.Unk30}");
            tw.WriteLine($"Unk34: {dummy.Unk34}");
            tw.PopScope();
        }

        static void PrintFlverLayoutMember(ScopeWriter tw, FLVER.LayoutMember layoutMember, int index)
        {
            tw.ScopeTitleLine($"LayoutMember: {index}", ScopeValue);
            tw.WriteLine($"GroupIndex: {layoutMember.GroupIndex}");
            tw.WriteLine($"Type: {layoutMember.Type}");
            tw.WriteLine($"Semantic: {layoutMember.Semantic}");
            tw.WriteLine($"Index: {layoutMember.Index}");
            tw.WriteLine($"Size: {layoutMember.Size}");
            tw.PopScope();
        }

        #endregion

        #region Error Helpers

        static void Warn(string value)
        {
            Console.WriteLine($"Warning: {value}");
            HadWarnings |= true;
        }

        static void Error(string value)
        {
            Console.WriteLine($"Error: {value}");
            HadErrors |= true;
        }

        #endregion
    }
}
