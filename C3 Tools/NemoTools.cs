using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using C3Tools.x360;
using NAudio.Midi;
using Encoder = System.Drawing.Imaging.Encoder;

namespace C3Tools
{
    public class NemoTools
    {
        #region Wii Stuff

        // // // // // // // // // // // // // // // 
        //WII STUFF WII STUFF WII STUFF WII STUFF // 
        // // // // // // // // // // // // // // // 
        
        /// <summary>
        /// Converts png_wii files to usable format
        /// </summary>
        /// <param name="wii_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <param name="output_path">Full path you'd like to save the converted image</param>
        /// <param name="format">Allowed formats: BMP | JPG | PNG (default)</param>
        /// <param name="delete_original">True: delete | False: keep (default)</param>
        /// <returns></returns>
        public bool ConvertWiiImage(string wii_image, string output_path, string format, bool delete_original)
        {
            var tplfile = Path.GetDirectoryName(wii_image) + "\\" + Path.GetFileNameWithoutExtension(wii_image) + ".tpl";
            var pngfile = tplfile.Replace(".tpl", ".png");
            var Headers = new ImageHeaders();

            isVerticalTexture = false;
            isHorizontalTexture = false;
            TextureSize = 128;

            DeleteFile(pngfile);
            
            try
            {
                if (tplfile != wii_image)
                {
                    DeleteFile(tplfile);

                    var binaryReader = new BinaryReader(File.OpenRead(wii_image));
                    var binaryWriter = new BinaryWriter(new FileStream(tplfile, FileMode.Create));

                    var wii_header = new byte[32];
                    binaryReader.Read(wii_header, 0, 32);

                    byte[] tpl_header;
                    if (wii_header.SequenceEqual(Headers.wii_128x256))
                    {
                        tpl_header = Headers.tpl_128x256;
                        isVerticalTexture = true;
                        TextureSize = 256;
                    }
                    else if (wii_header.SequenceEqual(Headers.wii_128x128_rgba32))
                    {
                        tpl_header = Headers.tpl_128x128_rgba32;
                    }
                    else if (wii_header.SequenceEqual(Headers.wii_256x256) ||
                             wii_header.SequenceEqual(Headers.wii_256x256_B) ||
                             wii_header.SequenceEqual(Headers.wii_256x256_c8))
                    {
                        tpl_header = Headers.tpl_256x256;
                        TextureSize = 256;
                    }
                    else if (wii_header.SequenceEqual(Headers.wii_128x128))
                    {
                        tpl_header = Headers.tpl_128x128;
                    }
                    else if (wii_header.SequenceEqual(Headers.wii_64x64))
                    {
                        TextureSize = 64;
                        tpl_header = Headers.tpl_64x64;
                    }
                    else
                    {
                        MessageBox.Show("File " + Path.GetFileName(wii_image) +
                            " has a header I don't recognize, so I can't convert it",
                            "Nemo Tools", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                    binaryWriter.Write(tpl_header);

                    var buffer = new byte[64];
                    int num;
                    do
                    {
                        num = binaryReader.Read(buffer, 0, 64);
                        if (num > 0)
                            binaryWriter.Write(buffer);
                    } while (num > 0);
                    binaryWriter.Dispose();
                    binaryReader.Dispose();
                }

                //this is so image quality is higher than the default
                var myEncoder = Encoder.Quality;
                var myEncoderParameters = new EncoderParameters(1);
                var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;

                var img = TPL.ConvertFromTPL(tplfile);
                img.Save(pngfile, GetEncoderInfo("image/png"), myEncoderParameters);
                img.Dispose();
                
                if (!File.Exists(pngfile))
                {
                    if (tplfile != wii_image)
                    {
                        DeleteFile(tplfile);
                    }
                    return false;
                }
                if (!format.ToLowerInvariant().Contains("png"))
                {
                    var image = NemoLoadImage(pngfile);
                    if (!ResizeImage(pngfile, image.Width, format, output_path))
                    {
                        image.Dispose();
                        DeleteFile(pngfile);
                        return false;
                    }
                    image.Dispose();
                }

                if (tplfile != wii_image && !KeepDDS)
                {
                    DeleteFile(tplfile);
                }
                if (!format.ToLowerInvariant().Contains("png"))
                {
                    DeleteFile(pngfile);
                }
                if (delete_original)
                {
                    SendtoTrash(wii_image);
                }
                return true;
            }
            catch (Exception)
            {
                if (tplfile != wii_image)
                {
                    DeleteFile(tplfile);
                }
                DeleteFile(pngfile);
                return false;
            }
        }

        /// <summary>
        /// Converts png_wii files to usable format
        /// </summary>
        /// <param name="wii_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <param name="output_path">Full path you'd like to save the converted image</param>
        /// <param name="format">Allowed formats: BMP | JPG | PNG (default)</param>
        /// <returns></returns>
        public bool ConvertWiiImage(string wii_image, string output_path, string format)
        {
            return ConvertWiiImage(wii_image, output_path,format, false);
        }
        
        /// <summary>
        /// Converts png_wii files to png format
        /// </summary>
        /// <param name="wii_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <param name="output_path">Full path you'd like to save the converted image</param>
        /// <returns></returns>
        public bool ConvertWiiImage(string wii_image, string output_path)
        {
            return ConvertWiiImage(wii_image, output_path, "png", false);
        }

        /// <summary>
        /// Converts png_wii files to png format
        /// </summary>
        /// <param name="wii_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <returns></returns>
        public bool ConvertWiiImage(string wii_image)
        {
            return ConvertWiiImage(wii_image, wii_image, "png", false);
        }

        /// <summary>
        /// Converts png_wii files to png format
        /// </summary>
        /// <param name="wii_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <param name="delete_original">True - delete | False - keep (default)</param>
        /// <returns></returns>
        public bool ConvertWiiImage(string wii_image, bool delete_original)
        {
            return ConvertWiiImage(wii_image, wii_image, "png", delete_original);
        }

        /// <summary>
        /// Converts images to png_wii format
        /// </summary>
        /// <param name="wimgt_path">Full path to wimgt.exe (REQUIRED)</param>
        /// <param name="image_path">Full path of image to be converted</param>
        /// <param name="output_path">Full path of output image</param>
        /// <param name="delete_original">True: Delete | False: Keep (default)</param>
        /// <returns></returns>
        public bool ConvertImagetoWii(string wimgt_path, string image_path, string output_path, bool delete_original)
        {
            var pngfile = Path.GetDirectoryName(image_path) + "\\" + Path.GetFileNameWithoutExtension(image_path) + ".png";
            var tplfile = Path.GetDirectoryName(image_path) + "\\" + Path.GetFileNameWithoutExtension(image_path) + ".tpl";
            var origfile = image_path;
            var Headers = new ImageHeaders();

            try
            {
                var ext = Path.GetExtension(image_path);
                if (ext == ".png_xbox" || ext == ".png_ps3")
                {
                    if (!ConvertRBImage(image_path, pngfile, "png", false))
                    {
                        return false;
                    }
                    image_path = pngfile;
                }
                if (!ResizeImage(image_path, 256, "png", pngfile))
                {
                    return false;
                }
                
                if (File.Exists(wimgt_path))
                {
                    if (image_path != tplfile)
                    {
                        DeleteFile(tplfile);
                        
                        try
                        {
                            var arg = "-d \"" + tplfile + "\" ENC -x TPL.CMPR \"" + pngfile + "\"";
                            var startInfo = new ProcessStartInfo
                                {
                                    CreateNoWindow = true,
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    FileName = wimgt_path,
                                    Arguments = arg,
                                    WorkingDirectory = Application.StartupPath + "\\bin\\"
                                };
                            var process = Process.Start(startInfo);
                            do
                            {
                                //
                            } while (!process.HasExited);
                            process.Dispose();

                            if (!File.Exists(tplfile))
                            {
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("There was an error in converting the png_wii file\nThe error was caused by wimgt.exe\nThe error says:" +
                                ex.Message, "Nemo Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            if (image_path != tplfile)
                            {
                                DeleteFile(tplfile);
                            }
                            if (image_path != pngfile)
                            {
                                DeleteFile(pngfile);
                            }
                        }
                    }

                    var wiifile = Path.GetDirectoryName(origfile) + "\\" + Path.GetFileNameWithoutExtension(origfile) + "_keep.png_wii";
                    wiifile = wiifile.Replace("_keep_keep", "_keep"); //in case of double _keep markers for whatever reason

                    DeleteFile(wiifile);
                    if (origfile != pngfile)
                    {
                        DeleteFile(pngfile);
                    }

                    var binaryReader = new BinaryReader(File.OpenRead(tplfile));
                    var binaryWriter = new BinaryWriter(new FileStream(wiifile, FileMode.Create));
                    binaryReader.BaseStream.Position = 64L;
                    binaryWriter.Write(Headers.wii_256x256);
                    var buffer = new byte[64];

                    int num;
                    do
                    {
                        num = binaryReader.Read(buffer, 0, 64);
                        if (num > 0)
                            binaryWriter.Write(buffer);
                    } while (num > 0);
                    binaryWriter.Dispose();
                    binaryReader.Dispose();

                    if (image_path != tplfile && !KeepDDS)
                    {
                        DeleteFile(tplfile);
                    }
                    if (delete_original)
                    {
                        DeleteFile(origfile);
                    }
                    return File.Exists(wiifile);
                }
                MessageBox.Show("Wimgt.exe is missing and is required\nNo png_wii album art was created", "Nemo Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                DeleteFile(pngfile);
                DeleteFile(tplfile);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Converts images to png_wii format
        /// </summary>
        /// <param name="wimgt_path">Full path to wimgt.exe (REQUIRED)</param>
        /// <param name="image_path">Full path of image to be converted</param>
        /// <param name="output_path">Full path of output image</param>
        /// <returns></returns>
        public bool ConvertImagetoWii(string wimgt_path, string image_path, string output_path)
        {
            return ConvertImagetoWii(wimgt_path, image_path, output_path, false);
        }

        /// <summary>
        /// Converts images to png_wii format
        /// </summary>
        /// <param name="wimgt_path">Full path to wimgt.exe (REQUIRED)</param>
        /// <param name="image_path">Full path of image to be converted</param>
        /// <returns></returns>
        public bool ConvertImagetoWii(string wimgt_path, string image_path)
        {
            return ConvertImagetoWii(wimgt_path, image_path, image_path, false);
        }

        public void ProcessBINFile(string file, string title)
        {
            try
            {
                var bin = File.ReadAllBytes(file);
                var header = new byte[] { 0x00, 0x00, 0x00, 0x70 };
                var head = new[] { bin[0], bin[1], bin[2], bin[3] };
                if (!head.SequenceEqual(header))
                {
                    MessageBox.Show("That is not a valid BIN file: incorrect header", title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                var sfd = new SaveFileDialog
                {
                    Filter = "Text File (*.txt)|*.txt",
                    Title = "Where should I save the ng_id.txt file?",
                    FileName = "ng_id.txt",
                    InitialDirectory = CurrentFolder
                };

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var idfile = sfd.FileName;
                if (!idfile.EndsWith(".txt", StringComparison.Ordinal))
                {
                    idfile = idfile + ".txt";
                }
                DeleteFile(idfile);

                var idbytes = new[] { bin[8], bin[9], bin[10], bin[11] };

                var sw = new StreamWriter(idfile, false);
                sw.Write((idbytes[0].ToString("X2") + idbytes[1].ToString("X2") + idbytes[2].ToString("X2") + idbytes[3].ToString("X2")).ToLowerInvariant());
                sw.Dispose();

                MessageBox.Show("Saved ng_id successfully", title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing BIN file: \n" + ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        // // // // // // // // // // // // // // // 
        //WII STUFF WII STUFF WII STUFF WII STUFF // 
        // // // // // // // // // // // // // // // 
        #endregion

        #region Declarations

        public string CurrentFolder = ""; //used throughout the program to maintain the current working folder
        public string MIDI_ERROR_MESSAGE;
        public bool isHorizontalTexture;
        public bool isVerticalTexture;
        private int TextureDivider = 2; //default value
        public int TextureSize = 512; //default value
        public bool KeepDDS = false;
        public bool isSaveFileCharacter = false; //need a different header
        public bool isSaveFileArt = false; //need to disable mip maps
        public string SaveFileBandName;
        public List<string> SaveFileCharNames;
        private const int FO_DELETE = 0x0003;
        private const int FOF_ALLOWUNDO = 0x0040;           // Preserve undo information, if possible.
        private const int FOF_NOCONFIRMATION = 0x0010;      // Show no confirmation dialog box to the user
        public string DDS_Format;
        
        // Struct which contains information that the SHFileOperation function uses to perform file operations.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public int wFunc;
            public string pFrom;
            public string pTo;
            public short fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);
        #endregion

        #region C3 Password Stuff

        public bool HasCorrectPassword(string authFile, string authPassword)
        {
            //REDACTED BY TROJANNEMO
            return false;
        }

        public bool IsAuthorized(bool doCrypt = false)
        {
            //REDACTED BY TROJANNEMO
            return false;
        }

        public bool HasMasterPassword()
        {
            //REDACTED BY TROJANNEMO
            return false;
        }

        public void SaveC3Password(string password)
        {
            //REDACTED BY TROJANNEMO
        }

        public string EncryptC3Password(string password)
        {
            //REDACTED BY TROJANNEMO
            return password;
        }

        public bool ConfirmPasswordMatches(string pass, bool doCrypt = false)
        {
            //REDACTED BY TROJANNEMO
            return false;
        }

        public bool GetPassword(bool doCrypt = false)
        {
            //REDACTED BY TROJANNEMO
            return false;
        }
        #endregion

        #region Image Stuff

        public ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            var encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        private const string ClientId = "GET YOUR OWN FROM IMGUR"; //from imgur, specific to this program, do not use elsewhere
        public string UploadToImgur(string image)
        {
            //remove code snippet below once you get your own code from imgur and enter above
            MessageBox.Show("You tried to upload to Imgur but haven't added your own Imgur code. Sign up for app access with Imgur then go to source code under 'UploadtoImgur' in 'NemoTools.cs' and enter your ClientID there and remove this pop-up message.",
                "HEY YOU", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return "";
            //remove code snippet above once you get your own code from imgur and enter above
            var link = "";
            try
            {
                using (var w = new WebClient())
                {
                    var values = new NameValueCollection
                        {
                            {"image", Convert.ToBase64String(File.ReadAllBytes(image))}
                        };
                    w.Headers.Add("Authorization", "Client-ID " + ClientId);
                    var response = w.UploadValues("https://api.imgur.com/3/upload.xml", values);
                    var sr = new StreamReader(new MemoryStream(response), Encoding.Default);
                    while (sr.Peek() >= 0)
                    {
                        var line = sr.ReadLine();
                        if (line == null || !line.Contains("link")) continue;
                        //get substring starting at http
                        line = line.Substring(line.IndexOf(":", StringComparison.Ordinal) - 4, line.Length - line.IndexOf(":", StringComparison.Ordinal));
                        //split string starting at </link
                        line = line.Substring(0, line.IndexOf("<", StringComparison.Ordinal));
                        link = line;
                    }
                    sr.Dispose();
                }
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                link = "";
                if (error.Contains("429"))
                {
                    error = "Error Code 429: Rate limiting\nThis most likely means C3 users have uploaded too many images recently\nPlease wait a couple of hours and try again";
                }
                else if (error.Contains("500"))
                {
                    error = "Error Code 500: Unexpected internal error\nThis most likely means something is broken with the Imgur service\nPlease try again later";
                }
                if (MessageBox.Show("Sorry, there was an error uploading that image!\n\nThe error says:\n" + error + "\n\nTry again?", "Error",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    UploadToImgur(image);
                }
            }
            return link;
        }

        /// <summary>
        /// Loads image and unlocks file for uses elsewhere. USE THIS!
        /// </summary>
        /// <param name="file">Full path to the image file</param>
        /// <returns></returns>
        public Image NemoLoadImage(string file)
        {
            if (!File.Exists(file))
            {
                return null;
            }

            Image img;
            using (var bmpTemp = new Bitmap(file))
            {
                img = new Bitmap(bmpTemp);
            }

            return img;
        }
        
        /// <summary>
        /// Returns true after successful conversion to png_xbox format
        /// </summary>
        /// <param name="image_path">Full path to original image</param>
        /// <param name="output_path">Full path of output png_xbox file</param>
        /// <param name="delete_original">True to delete original image | False to keep original image</param>
        /// <param name="do_ps3">Set true to output png_ps3 image, false to output png_xbox (default)</param>
        /// <returns></returns>
        public bool ConvertImagetoRB(string image_path, string output_path, bool delete_original, bool do_ps3 = false)
        {
            var ddsfile = Path.GetDirectoryName(image_path) + "\\" + Path.GetFileNameWithoutExtension(image_path) + ".dds";
            var keep = isSaveFileArt || isSaveFileCharacter ? "" : "_keep";
            var outputfile = Path.GetDirectoryName(output_path) + "\\" + Path.GetFileNameWithoutExtension(output_path) + keep + ".png_" + (do_ps3 ? "ps3" : "xbox");
            outputfile = outputfile.Replace("_keep_keep", "_keep"); //in case it already had it and was added above
            var tgafile = Application.StartupPath + "\\bin\\temp.tga";
            var Headers = new ImageHeaders();

            var nv_tool = Application.StartupPath + "\\bin\\nvcompress.exe";
            if (!File.Exists(nv_tool))
            {
                MessageBox.Show("nvcompress.exe is missing and is required\nProcess aborted", "Nemo Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                if (ddsfile != image_path)
                {
                    DeleteFile(ddsfile);
                }
                DeleteFile(outputfile);
                DeleteFile(tgafile);

                if (!image_path.EndsWith(".dds", StringComparison.Ordinal)) //allow for .dds input image for superior quality
                {
                    if (!ResizeImage(image_path, TextureSize, "tga", tgafile))
                    {
                        return false;
                    }

                    try
                    {
                        var mip = isSaveFileArt || isSaveFileCharacter ? "-nomips " : ""; //need to disable mips, otherwise always enable
                        //save as 512x512 / 1024x1024 DXT5 textures - first time ever in RB3 customs @ TrojanNemo 2014 bitches
                        var arg = mip + "-nocuda -bc3 \"" + tgafile + "\" \"" + ddsfile + "\"";
                        var startInfo = new ProcessStartInfo
                        {
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            FileName = nv_tool,
                            Arguments = arg,
                            WorkingDirectory = Application.StartupPath + "\\bin\\"
                        };
                        var process = Process.Start(startInfo);
                        do
                        {
                            //
                        } while (!process.HasExited);
                        process.Dispose();

                        if (!File.Exists(ddsfile))
                        {
                            return false;
                        }
                        DeleteFile(tgafile);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                else
                {
                    ddsfile = image_path;
                }

                //read all raw bytes
                var ddsbytes = File.ReadAllBytes(ddsfile);
                var buffer = new byte[4];
                var swap = new byte[4];

                //default header
                var header = Headers.RB3_512x512_DXT5;
                if (isSaveFileCharacter)
                {
                    header = Headers.RB3_256x512_DXT5_NOMIP;
                }
                else if (isSaveFileArt)
                {
                    header = Headers.RB3_256x256_DXT5_NOMIP;
                }
                else
                {
                    switch (TextureSize)
                    {
                        case 256:
                            header = Headers.RB3_256x256_DXT5;
                            break;
                        case 1024:
                            header = Headers.NEMO_1024x1024_DXT5;
                            break;
                        case 2048:
                            header = Headers.NEMO_2048x2048_DXT5;
                            break;
                    }
                }

                //get filesize / 4 for number of times to loop
                //128 is size of dds header we have to skip
                var loop = (ddsbytes.Length - 128) / 4;

                //skip the first dds header
                var input = new MemoryStream(ddsbytes, 128, ddsbytes.Length - 128);
                var output = new FileStream(outputfile, FileMode.Create);

                //write HMX header
                output.Write(header, 0, header.Length);

                //here we go
                for (var x = 0; x <= loop; x++)
                {
                    input.Read(buffer, 0, 4);
                    //png_ps3 are not byte swapped, no need to change anything
                    if (do_ps3)
                    {
                        swap = buffer;
                    }
                    else
                    {
                        //png_xbox are byte swapped, so got to change it here
                        swap[0] = buffer[1];
                        swap[1] = buffer[0];
                        swap[2] = buffer[3];
                        swap[3] = buffer[2];
                    }
                    output.Write(swap, 0, 4);
                }
                input.Dispose();
                output.Dispose();

                //clean up temporary file silently
                if (!image_path.EndsWith(".dds", StringComparison.Ordinal) && !KeepDDS)
                {
                    DeleteFile(ddsfile);
                }
                if (delete_original)
                {
                    SendtoTrash(image_path);
                }
                if (File.Exists(outputfile))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                if (!image_path.EndsWith(".dds", StringComparison.Ordinal))
                {
                    DeleteFile(ddsfile);
                }
                DeleteFile(tgafile);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Returns true after successful conversion to png_xbox format
        /// </summary>
        /// <param name="image_path">Full path to original image</param>
        /// <param name="output_path">Full path of output png_xbox file</param>
        /// <returns></returns>
        public bool ConvertImagetoRB(string image_path, string output_path)
        {
            return ConvertImagetoRB(image_path, output_path, false);
        }

        /// <summary>
        /// Returns true after successful conversion to png_xbox format
        /// </summary>
        /// <param name="image_path">Full path to original image</param>
        /// <returns></returns>
        public bool ConvertImagetoRB(string image_path)
        {
            return ConvertImagetoRB(image_path, image_path, false);
        }

        /// <summary>
        /// Returns true after successful conversion to png_xbox format
        /// </summary>
        /// <param name="image_path">Full path to original image</param>
        /// <param name="delete_original">True - delete | False - keep (default)</param>
        /// <returns></returns>
        public bool ConvertImagetoRB(string image_path, bool delete_original)
        {
            return ConvertImagetoRB(image_path, image_path, delete_original);
        }

        /// <summary>
        /// Use to resize images up or down or convert across BMP/JPG/PNG/TIF
        /// </summary>
        /// <param name="image_path">Full file path to source image</param>
        /// <param name="image_size">Integer for image size, can be smaller or bigger than source image</param>
        /// <param name="format">Format to save the image in: BMP | JPG | TIF | PNG (default)</param>
        /// <param name="output_path">Full file path to output image</param>
        /// <returns></returns>
        public bool ResizeImage(string image_path, int image_size, string format, string output_path)
        {
            try
            {
                var newimage = Path.GetDirectoryName(output_path) + "\\" + Path.GetFileNameWithoutExtension(output_path);

                Il.ilInit();
                Ilu.iluInit();

                var imageId = new int[10];

                // Generate the main image name to use
                Il.ilGenImages(1, imageId);

                // Bind this image name
                Il.ilBindImage(imageId[0]);

                // Loads the image into the imageId
                if (!Il.ilLoadImage(image_path))
                {
                    return false;
                }
                // Enable overwriting destination file
                Il.ilEnable(Il.IL_FILE_OVERWRITE);

                var height = isHorizontalTexture ? image_size / TextureDivider : image_size;
                var width = isVerticalTexture ? image_size / TextureDivider : image_size;

                //assume we're downscaling, this is better filter
                const int scaler = Ilu.ILU_BILINEAR;

                //resize image
                Ilu.iluImageParameter(Ilu.ILU_FILTER, scaler);
                Ilu.iluScale(width, height, 1);

                if (format.ToLowerInvariant().Contains("bmp"))
                {
                    //disable compression
                    Il.ilSetInteger(Il.IL_BMP_RLE, 0);
                    newimage = newimage + ".bmp";
                }
                else if (format.ToLowerInvariant().Contains("jpg") || format.ToLowerInvariant().Contains("jpeg"))
                {
                    Il.ilSetInteger(Il.IL_JPG_QUALITY, 99);
                    newimage = newimage + ".jpg";
                }
                else if (format.ToLowerInvariant().Contains("tif"))
                {
                    newimage = newimage + ".tif";
                }
                else if (format.ToLowerInvariant().Contains("tga"))
                {
                    Il.ilSetInteger(Il.IL_TGA_RLE, 0);
                    newimage = newimage + ".tga";
                }
                else
                {
                    Il.ilSetInteger(Il.IL_PNG_INTERLACE, 0);
                    newimage = newimage + ".png";
                }

                if (!Il.ilSaveImage(newimage))
                {
                    return false;
                }

                // Done with the imageId, so let's delete it
                Il.ilDeleteImages(1, imageId);

                return File.Exists(newimage);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Use to resize images up or down or convert across BMP/JPG/PNG
        /// </summary>
        /// <param name="image_path">Full file path to source image</param>
        /// <param name="image_size">Integer for image size, can be smaller or bigger than source image</param>
        /// <param name="format">Format to save the image in: BMP | JPG | PNG (default)</param>
        /// <returns></returns>
        public bool ResizeImage(string image_path, int image_size, string format)
        {
            return ResizeImage(image_path, image_size, format, image_path);
        }

        /// <summary>
        /// Use to resize images up or down or convert across BMP/JPG/PNG
        /// </summary>
        /// <param name="image_path">Full file path to source image</param>
        /// <param name="image_size">Integer for image size, can be smaller or bigger than source image</param>
        /// <returns></returns>
        public bool ResizeImage(string image_path, int image_size)
        {
            var img = NemoLoadImage(image_path);
            var format = img.RawFormat.ToString();
            img.Dispose();

            return ResizeImage(image_path, image_size, format);
        }

        /// <summary>
        /// Converts png_xbox image to png_ps3 image
        /// </summary>
        /// <param name="image">Full file path to the image to be converted</param>
        /// <param name="output_path">Full directory path where new image is to be saved</param>
        /// <param name="delete_original">Whether to delete the original image upon completion</param>
        /// <returns></returns>
        public bool ConvertXboxtoPS3(string image, string output_path, bool delete_original)
        {
            return FlipRBImageBytes(image, output_path, "ps3", delete_original);
        }

        /// <summary>
        /// Converts png_ps3 image to png_xbox image
        /// </summary>
        /// <param name="image">Full file path to the image to be converted</param>
        /// <param name="output_path">Full directory path where new image is to be saved</param>
        /// <param name="delete_original">Whether to delete the original image upon completion</param>
        /// <returns></returns>
        public bool ConvertPS3toXbox(string image, string output_path, bool delete_original)
        {
            return FlipRBImageBytes(image, output_path, "xbox", delete_original);
        }

        /// <summary>
        /// Converts png_xbox to/from png_ps3
        /// </summary>
        /// <param name="image">Full file path to the image to convert</param>
        /// <param name="output_path">Full directory path where new image is to be saved</param>
        /// <param name="extension">Either 'xbox' or 'ps3'</param>
        /// <param name="delete_original">Whether to delete the original</param>
        /// <returns></returns>
        private bool FlipRBImageBytes(string image, string output_path, string extension, bool delete_original)
        {
            try
            {
                var output_image = Path.GetDirectoryName(output_path) + "\\" + Path.GetFileNameWithoutExtension(output_path) + ".png_" + extension.ToLowerInvariant();

                if (output_image == image)
                {
                    return true; //why are you wasting my time?
                }

                //read all raw bytes
                var ddsbytes = File.ReadAllBytes(image);
                var buffer = new byte[4];
                var swap = new byte[4];

                //get filesize / 4 for number of times to loop
                //32 is size of HMX header
                var loop = (ddsbytes.Length - 32) / 4;

                //grab the header
                var header = new byte[32];
                var header_stream = new MemoryStream(ddsbytes, 0, 32);
                header_stream.Read(header, 0, 32);

                //skip the HMX header = leaves us with image bytes
                var input = new MemoryStream(ddsbytes, 32, ddsbytes.Length - 32);

                //create new image
                var output = new FileStream(output_image, FileMode.Create);

                //both png_xbox and png_ps3 files use the same header, put it back
                output.Write(header, 0, header.Length);

                //here we go
                for (var x = 0; x <= loop; x++)
                {
                    input.Read(buffer, 0, 4);

                    swap[0] = buffer[1];
                    swap[1] = buffer[0];
                    swap[2] = buffer[3];
                    swap[3] = buffer[2];

                    output.Write(swap, 0x00, 4);
                }
                input.Dispose();
                output.Dispose();

                if (delete_original)
                {
                    DeleteFile(image);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static byte[] BuildDDSHeader(string format, int width, int height)
        {
            var dds = new byte[] //512x512 DXT5
                {
                    0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x0A, 0x00, 0x00, 0x02, 0x00, 0x00, 
                    0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                    0x00, 0x00, 0x00, 0x00, 0x4E, 0x45, 0x4D, 0x4F, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00, 
                    0x04, 0x00, 0x00, 0x00, 0x44, 0x58, 0x54, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };

            switch (format.ToLowerInvariant())
            {
                case "dxt1":
                    dds[87] = 0x31;
                    break;
                case "dxt3":
                    dds[87] = 0x33;
                    break;
                case "normal":
                    dds[84] = 0x41;
                    dds[85] = 0x54;
                    dds[86] = 0x49;
                    dds[87] = 0x32;
                    break;
            }

            switch (height)
            {
                case 8:
                    dds[12] = 0x08;
                    dds[13] = 0x00;
                    break;
                case 16:
                    dds[12] = 0x10;
                    dds[13] = 0x00;
                    break;
                case 32:
                    dds[12] = 0x20;
                    dds[13] = 0x00;
                    break;
                case 64:
                    dds[12] = 0x40;
                    dds[13] = 0x00;
                    break;
                case 128:
                    dds[12] = 0x80;
                    dds[13] = 0x00;
                    break;
                case 256:
                    dds[13] = 0x01;
                    break;
                case 1024:
                    dds[13] = 0x04;
                    break;
                case 2048:
                    dds[13] = 0x08;
                    break;
            }

            switch (width)
            {
                case 8:
                    dds[16] = 0x08;
                    dds[17] = 0x00;
                    break;
                case 16:
                    dds[16] = 0x10;
                    dds[17] = 0x00;
                    break;
                case 32:
                    dds[16] = 0x20;
                    dds[17] = 0x00;
                    break;
                case 64:
                    dds[16] = 0x40;
                    dds[17] = 0x00;
                    break;
                case 128:
                    dds[16] = 0x80;
                    dds[17] = 0x00;
                    break;
                case 256:
                    dds[17] = 0x01;
                    break;
                case 1024:
                    dds[17] = 0x04;
                    break;
                case 2048:
                    dds[17] = 0x08;
                    break;
            }
            return dds;
        }

        /// <summary>
        /// Figure out right DDS header to go with HMX texture
        /// </summary>
        /// <param name="full_header">First 16 bytes of the png_xbox/png_ps3 file</param>
        /// <param name="short_header">Bytes 5-16 of the png_xbox/png_ps3 file</param>
        /// <returns></returns>
        private byte[] GetDDSHeader(IEnumerable<byte> full_header, IEnumerable<byte> short_header)
        {
            //official album art header, most likely to be the one being requested
            var header = BuildDDSHeader("dxt1", 256, 256);

            var headers = Directory.GetFiles(Application.StartupPath + "\\bin\\headers\\", "*.header");
            DDS_Format = "UNKNOWN";
            foreach (var head_name in from head in headers let header_bytes = File.ReadAllBytes(head) where full_header.SequenceEqual(header_bytes) || short_header.SequenceEqual(header_bytes) select Path.GetFileNameWithoutExtension(head).ToLowerInvariant())
            {
                DDS_Format = "DXT5";
                if (head_name.Contains("dxt1"))
                {
                    DDS_Format = "DXT1";
                }
                else if (head_name.Contains("normal"))
                {
                    DDS_Format = "NORMAL_MAP";
                }

                var index1 = head_name.IndexOf("_", StringComparison.Ordinal) + 1;
                var index2 = head_name.IndexOf("x", StringComparison.Ordinal);
                var width = Convert.ToInt16(head_name.Substring(index1, index2 - index1));
                index1 = head_name.IndexOf("_", index2, StringComparison.Ordinal);
                index2++;
                var height = Convert.ToInt16(head_name.Substring(index2, index1 - index2));

                header = BuildDDSHeader(DDS_Format.ToLowerInvariant().Replace("_map", ""), width, height);
                break;
            }
            return header;
        }

        /// <summary>
        /// Converts png_xbox files to usable format
        /// </summary>
        /// <param name="rb_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <param name="output_path">Full path you'd like to save the converted image</param>
        /// <param name="format">Allowed formats: BMP | JPG | PNG (default)</param>
        /// <param name="delete_original">True: delete | False: keep (default)</param>
        /// <returns></returns>
        public bool ConvertRBImage(string rb_image, string output_path, string format, bool delete_original)
        {
            var ddsfile = Path.GetDirectoryName(output_path) + "\\" + Path.GetFileNameWithoutExtension(output_path) + ".dds";
            var tgafile = ddsfile.Replace(".dds", ".tga");

            TextureSize = 256; //default size album art
            TextureDivider = 2; //default to divide larger size by, always multiples of 2
            isHorizontalTexture = false; //this is a rectangle wider than tall
            isVerticalTexture = false; //this is a rectangle taller than wide

            var nv_tool = Application.StartupPath + "\\bin\\nvdecompress.exe";
            if (!File.Exists(nv_tool))
            {
                MessageBox.Show("nvdecompress.exe is missing and is required\nProcess aborted", "Nemo Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            try
            {
                if (ddsfile != rb_image)
                {
                    DeleteFile(ddsfile);
                }
                DeleteFile(tgafile);

                //read raw file bytes
                var ddsbytes = File.ReadAllBytes(rb_image);

                if (!rb_image.EndsWith(".dds", StringComparison.Ordinal))
                {
                    var buffer = new byte[4];
                    var swap = new byte[4];

                    //get filesize / 4 for number of times to loop
                    //32 is the size of the HMX header to skip
                    var loop = (ddsbytes.Length - 32) / 4;

                    //skip the HMX header
                    var input = new MemoryStream(ddsbytes, 32, ddsbytes.Length - 32);

                    //grab HMX header to compare against known headers
                    var full_header = new byte[16];
                    var file_header = new MemoryStream(ddsbytes, 0, 16);
                    file_header.Read(full_header, 0, 16);
                    file_header.Dispose();

                    //some games have a bunch of headers for the same files, so let's skip the varying portion and just
                    //grab the part that tells us the dimensions and image format
                    var short_header = new byte[11];
                    file_header = new MemoryStream(ddsbytes, 5, 11);
                    file_header.Read(short_header, 0, 11);
                    file_header.Dispose();

                    //create dds file
                    var output = new FileStream(ddsfile, FileMode.Create);
                    var header = GetDDSHeader(full_header, short_header);
                    output.Write(header, 0, header.Length);

                    //here we go
                    for (var x = 0; x <= loop; x++)
                    {
                        input.Read(buffer, 0, 4);

                        //PS3 images are not byte swapped, just DDS images with HMX header on top
                        if (rb_image.EndsWith("_ps3", StringComparison.Ordinal))
                        {
                            swap = buffer;
                        }
                        else
                        {
                            //XBOX images are byte swapped, so we gotta return it
                            swap[0] = buffer[1];
                            swap[1] = buffer[0];
                            swap[2] = buffer[3];
                            swap[3] = buffer[2];
                        }
                        output.Write(swap, 0, 4);
                    }
                    input.Dispose();
                    output.Dispose();
                }
                else
                {
                    ddsfile = rb_image;
                }

                //read raw dds bytes
                ddsbytes = File.ReadAllBytes(ddsfile);

                //grab relevant part of dds header
                var header_stream = new MemoryStream(ddsbytes, 0, 32);
                var size = new byte[32];
                header_stream.Read(size, 0, 32);
                header_stream.Dispose();

                //default to 256x256
                var width = 256;
                var height = 256;

                //get dds dimensions from header
                switch (size[17]) //width byte
                {
                    case 0x00:
                        switch (size[16])
                        {
                            case 0x08:
                                width = 8;
                                break;
                            case 0x10:
                                width = 16;
                                break;
                            case 0x20:
                                width = 32;
                                break;
                            case 0x40:
                                width = 64;
                                break;
                            case 0x80:
                                width = 128;
                                break;
                        }
                        break;
                    case 0x02:
                        width = 512;
                        break;
                    case 0x04:
                        width = 1024;
                        break;
                    case 0x08:
                        width = 2048;
                        break;
                }
                switch (size[13]) //height byte
                {
                    case 0x00:
                        switch (size[12])
                        {
                            case 0x08:
                                height = 8;
                                break;
                            case 0x10:
                                height = 16;
                                break;
                            case 0x20:
                                height = 32;
                                break;
                            case 0x40:
                                height = 64;
                                break;
                            case 0x80:
                                height = 128;
                                break;
                        }
                        break;
                    case 0x02:
                        height = 512;
                        break;
                    case 0x04:
                        height = 1024;
                        break;
                    case 0x08:
                        height = 2048;
                        break;
                }

                if (width > height)
                {
                    isHorizontalTexture = true;
                    TextureDivider = width / height;
                    TextureSize = width;
                }
                else if (height > width)
                {
                    isVerticalTexture = true;
                    TextureDivider = height / width;
                    TextureSize = height;
                }
                else
                {
                    TextureSize = width;
                }

                var arg = "\"" + ddsfile + "\"";
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = nv_tool,
                    Arguments = arg,
                    WorkingDirectory = Application.StartupPath + "\\bin\\"
                };
                var process = Process.Start(startInfo);
                do
                {
                    //
                } while (!process.HasExited);
                process.Dispose();

                if (!ResizeImage(tgafile, TextureSize, format, output_path))
                {
                    DeleteFile(tgafile);
                    return false;
                }
                if (!rb_image.EndsWith(".dds", StringComparison.Ordinal) && !KeepDDS)
                {
                    DeleteFile(ddsfile);
                }
                if (!format.ToLowerInvariant().Contains("tga"))
                {
                    DeleteFile(tgafile);
                }
                if (delete_original)
                {
                    SendtoTrash(rb_image);
                }
                return true;
            }
            catch (Exception)
            {
                if (!rb_image.EndsWith(".dds", StringComparison.Ordinal))
                {
                    DeleteFile(ddsfile);
                }
                return false;
            }
        }

        /// <summary>
        /// Converts png_xbox files to usable format
        /// </summary>
        /// <param name="rb_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <param name="output_path">Full path you'd like to save the converted image</param>
        /// <param name="format">Allowed formats: BMP | JPG | PNG (default)</param>
        /// <returns></returns>
        public bool ConvertRBImage(string rb_image, string output_path, string format)
        {
            return ConvertRBImage(rb_image, output_path, format, false);
        }

        /// <summary>
        /// Converts png_xbox files to usable format
        /// </summary>
        /// <param name="rb_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <param name="output_path">Full path you'd like to save the converted image</param>
        /// <returns></returns>
        public bool ConvertRBImage(string rb_image, string output_path)
        {
            return ConvertRBImage(rb_image, output_path, "png", false);
        }

        /// <summary>
        /// Converts png_xbox files to usable format
        /// </summary>
        /// <param name="rb_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <returns></returns>
        public bool ConvertRBImage(string rb_image)
        {
            return ConvertRBImage(rb_image, rb_image, "png", false);
        }

        /// <summary>
        /// Converts png_xbox files to usable format
        /// </summary>
        /// <param name="rb_image">Full path to the png_xbox / png_ps3 / dds file</param>
        /// <param name="delete_original">True - delete | False - keep (default)</param>
        /// <returns></returns>
        public bool ConvertRBImage(string rb_image, bool delete_original)
        {
            return ConvertRBImage(rb_image, rb_image, "png", delete_original);
        }
        #endregion

        #region Misc Stuff

        public int GetDiffTag(Control instrument)
        {
            int diff;
            try
            {
                diff = Convert.ToInt16(instrument.Tag);
            }
            catch (Exception)
            {
                return 0;
            }
            return diff;
        }

        public void ReplaceSongID(string dta, string songID, string vocals = "")
        {
            var sr = new StreamReader(dta);
            var utf8 = sr.ReadToEnd().ToLowerInvariant().Contains("utf8");
            sr.Dispose();
            sr = new StreamReader(dta, utf8 ? Encoding.UTF8 : Encoding.Default);
            var lines = new List<string>();
            while (sr.Peek() >= 0)
            {
                var line = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    lines.Add(line);
                }
            }
            sr.Dispose();
            var doVocals = !string.IsNullOrWhiteSpace(vocals) && vocals != "0";
            var sw = new StreamWriter(dta, false, utf8 ? Encoding.UTF8 : Encoding.Default);
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("song_id")) continue;
                if (lines[i].Contains("vocal_parts") && doVocals) continue;
                if ((lines[i].Trim() == "(song" || lines[i].Trim() == "'song'") && doVocals)
                {
                    sw.WriteLine(lines[i]);
                    sw.WriteLine("      (vocal_parts " + vocals + ")");
                    continue;
                }
                if (i == lines.Count - 3)
                {
                    sw.WriteLine("   (song_id " + songID + ")");
                }
                sw.WriteLine(lines[i]);
            }
            sw.Dispose();
        }

        /// <summary>
        /// Returns line with featured artist normalized as 'ft.'
        /// </summary>
        /// <param name="line">Line to normalize</param>
        /// <returns></returns>
        public string FixFeaturedArtist(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return "";

            var adjusted = line;

            adjusted = adjusted.Replace("Featuring", "ft.");
            adjusted = adjusted.Replace("featuring", "ft.");
            adjusted = adjusted.Replace("feat.", "ft.");
            adjusted = adjusted.Replace("Feat.", "ft.");
            adjusted = adjusted.Replace(" ft ", " ft. ");
            adjusted = adjusted.Replace(" FT ", " ft. ");
            adjusted = adjusted.Replace("Ft. ", "ft. ");
            adjusted = adjusted.Replace("FEAT. ", "ft. ");
            adjusted = adjusted.Replace(" FEAT ", " ft. ");

            if (adjusted.StartsWith("ft ", StringComparison.Ordinal))
            {
                adjusted = "ft. " + adjusted.Substring(3, adjusted.Length - 3);
            }

            return FixBadChars(adjusted);
        }

        /// <summary>
        /// Loads and formats help file for display on the HelpForm
        /// </summary>
        /// <param name="file">Name of the file, path assumed to be \bin\help/</param>
        /// <returns></returns>
        public string ReadHelpFile(string file)
        {
            var message = string.Empty;
            var helpfile = Application.StartupPath + "\\bin\\help\\" + file;
            if (File.Exists(helpfile))
            {
                var sr = new StreamReader(helpfile);
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    message = message == string.Empty ? line : message + "\r\n" + line;
                }
                sr.Dispose();
            }
            else
            {
                message = "Could not find help file, please redownload this program and DO NOT delete any files";
            }

            return message;
        }

        public Color LightenColor(Color color)
        {
            var correctionFactor = (float)0.20;

            var red = (float)color.R;
            var green = (float)color.G;
            var blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        public Color DarkenColor(Color color)
        {
            var correctionFactor = (float)-0.25;

            var red = (float)color.R;
            var green = (float)color.G;
            var blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }
            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }
        
        /// <summary>
        /// Use to quickly grab value on right side of = in C3 options/fix files
        /// </summary>
        /// <param name="raw_line">Raw line from the c3 file</param>
        /// <returns></returns>
        public string GetConfigString(string raw_line)
        {
            if (string.IsNullOrWhiteSpace(raw_line)) return "";
            var line = raw_line;
            try
            {
                var index = line.IndexOf("=", StringComparison.Ordinal) + 1;
                line = line.Substring(index, line.Length - index);
            }
            catch (Exception)
            {
                line = "";
            }
            return line.Trim();
        }
        
        /// <summary>
        /// Will send files or folders to Recycle Bin rather than delete from hard drive
        /// </summary>
        /// <param name="path">Full file / folder path to be recycled</param>
        /// <param name="isFolder">Whether path is to a folder rather than a file</param>
        public void SendtoTrash(string path, bool isFolder = false)
        {
            if (isFolder)
            {
                if (!Directory.Exists(path)) return;
            }
            else
            {
                if (!File.Exists(path)) return;
            }
            
            try
            {
                var fileop = new SHFILEOPSTRUCT
                {
                    wFunc = FO_DELETE,
                    pFrom = path + '\0' + '\0',
                    fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION
                };
                SHFileOperation(ref fileop);
            }
            catch (Exception)
            {}
        }
        
        /// <summary>
        /// Will safely try to move, and if fails, copy/delete a file
        /// </summary>
        /// <param name="input">Full starting path of the file</param>
        /// <param name="output">Full destination path of the file</param>
        public bool MoveFile(string input, string output)
        {
            try
            {
                DeleteFile(output);
                File.Move(input, output);
            }
            catch (Exception)
            {
                try
                {
                    File.Copy(input, output);
                    DeleteFile(input);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return File.Exists(output);
        }

        /// <summary>
        /// Simple function to safely delete folders
        /// </summary>
        /// <param name="folder">Full path of folder to be deleted</param>
        /// <param name="delete_contents">Whether to delete folders that are not empty</param>
        public void DeleteFolder(string folder, bool delete_contents)
        {
            if (!Directory.Exists(folder)) return;
            try
            {
                if (delete_contents)
                {
                    Directory.Delete(folder, true);
                    return;
                }
                if (!Directory.GetFiles(folder).Any())
                {
                    Directory.Delete(folder);
                }
            }
            catch (Exception)
            {}
        }

        /// <summary>
        /// Simple function to safely delete files
        /// </summary>
        /// <param name="file">Full path of file to be deleted</param>
        public void DeleteFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file)) return;
            if (!File.Exists(file)) return;
            try
            {
                File.Delete(file);
            }
            catch (Exception)
            {}
        }

        /// <summary>
        /// Simple function to safely delete folders
        /// </summary>
        /// <param name="folder">Full path of folder to be deleted</param>
        public void DeleteFolder(string folder)
        {
            if (!Directory.Exists(folder)) return;
            DeleteFolder(folder, false);
        }

        /// <summary>
        /// Use to send mogg to Audacity for editing
        /// </summary>
        /// <param name="mogg">Full file path to audio file</param>
        /// <returns>String message with either success or failure messages for logging/displaying</returns>
        public string SendtoAudacity(string mogg)
        {
            var mData = File.ReadAllBytes(mogg);
            if (mData[0] != 0x0A)
            {
                WriteOutData(ObfM(mData), mogg);
                return "Audio file " + Path.GetFileName(mogg) + " is encrypted and can't be sent to Audacity, sorry";
            }
            try
            {
                var audacity = "";
                if (File.Exists(Application.StartupPath + "\\bin\\audacity.c3"))
                {
                    var sr = new StreamReader(Application.StartupPath + "\\bin\\audacity.c3", Encoding.Default);
                    audacity = sr.ReadLine();
                    sr.Dispose();
                }
                if (string.IsNullOrWhiteSpace(audacity) || !File.Exists(audacity) || !audacity.EndsWith(".exe", StringComparison.Ordinal))
                {
                    if (File.Exists("C:\\Program Files (x86)\\Audacity\\audacity.exe"))
                    {
                        audacity = "C:\\Program Files (x86)\\Audacity\\audacity.exe";
                    }
                    else if (File.Exists("C:\\Program Files\\Audacity\\audacity.exe"))
                    {
                        audacity = "C:\\Program Files\\Audacity\\audacity.exe";
                    }
                    else
                    {
                        var openfile = new OpenFileDialog
                        {
                            Filter = "Windows Executable (*.exe)|*.exe",
                            InitialDirectory = Application.StartupPath,
                            Title = "Select Audacity executable",
                            FileName = "audacity"
                        };
                        openfile.ShowDialog();
                        if (openfile.FileName != "")
                        {
                            audacity = openfile.FileName;
                            var sw = new StreamWriter(Application.StartupPath + "\\audacity.c3", false, Encoding.Default);
                            sw.WriteLine(audacity);
                            sw.Dispose();
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(audacity) || !File.Exists(audacity) || !audacity.EndsWith(".exe", StringComparison.Ordinal))
                {
                    return "Could not find Audacity executable\nProcess aborted";
                }
                CurrentFolder = Path.GetDirectoryName(mogg) + "\\";
                var arg = "\"" + mogg + "\"";
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    UseShellExecute = false,
                    FileName = audacity,
                    Arguments = arg,
                    WorkingDirectory = Path.GetDirectoryName(mogg) + "\\"
                };
                var process = Process.Start(startInfo);
                do
                {
                    //wait
                } while (!process.HasExited);
                process.Dispose();
                return "Audacity process completed successfully";
            }
            catch (Exception ex)
            {
                return "There was an error while sending to Audacity\nThe error says: " + ex.Message;
            }
        }
        
        /// <summary>
        /// Unlocks STFS package to show as a full song in game
        /// </summary>
        /// <param name="file_path">Full path to the CON or LIVE file</param>
        /// <returns></returns>
        public bool UnlockCON(string file_path)
        {
            //open and unlock CON/LIVE package
            try
            {
                var bw = new BinaryWriter(File.Open(file_path, FileMode.Open, FileAccess.ReadWrite));
                bw.BaseStream.Seek(567L, SeekOrigin.Begin);
                bw.Write(0x01);
                bw.Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Signs STFS file as CON for use in retail Xboxes
        /// </summary>
        /// <param name="file_path">Full path to the STFS file to sign</param>
        /// <returns></returns>
        public bool SignCON(string file_path)
        {
            var xPackage = new STFSPackage(file_path);
            if (!xPackage.ParseSuccess)
            {
                return false;
            }
            try
            {
                var kv = new RSAParams(Application.StartupPath + "\\bin\\KV.bin");
                if (kv.Valid)
                {
                    xPackage.FlushPackage(kv);
                    xPackage.UpdateHeader(kv);
                    xPackage.CloseIO();
                    return true;
                }
                xPackage.CloseIO();
                return false;
            }
            catch
            {
                xPackage.CloseIO();
                return false;
            }
        }

        /// <summary>
        /// Returns string with correctly formatted characters
        /// </summary>
        /// <param name="raw_line">Raw line from songs.dta file</param>
        /// <returns></returns>
        public string FixBadChars(string raw_line)
        {
            var line = raw_line.Replace("Ã¡", "á");
            line = line.Replace("Ã©", "é");
            line = line.Replace("Ã¨", "è");
            line = line.Replace("ÃŠ", "Ê");
            line = line.Replace("Ã¬", "ì");
            line = line.Replace("Ã­­­", "í");
            line = line.Replace("Ã¯", "ï");
            line = line.Replace("Ã–", "Ö");
            line = line.Replace("Ã¶", "ö");
            line = line.Replace("Ã³", "ó");
            line = line.Replace("Ã²", "ò");
            line = line.Replace("Ãœ", "Ü");
            line = line.Replace("Ã¼", "ü");
            line = line.Replace("Ã¹", "ù");
            line = line.Replace("Ãº", "ú");
            line = line.Replace("Ã¿", "ÿ");
            line = line.Replace("Ã±", "ñ");
            line = line.Replace("ï¿½", "");
            line = line.Replace("�", "");
            line = line.Replace("E½", "");
            return line;
        }
        
        /// <summary>
        /// Returns byte array in hex value
        /// </summary>
        /// <param name="xIn">String value to be converted to hex</param>
        /// <returns></returns>
        public byte[] ToHex(string xIn)
        {
            for (var i = 0; i < (xIn.Length % 2); i++)
                xIn = "0" + xIn;
            var xReturn = new List<byte>();
            for (var i = 0; i < (xIn.Length / 2); i++)
                xReturn.Add(Convert.ToByte(xIn.Substring(i * 2, 2), 16));
            return xReturn.ToArray();
        }
        
        /// <summary>
        /// Returns true if the package description suggests a pack
        /// </summary>
        /// <param name="desc">Package description</param>
        /// <param name="disp">Package display</param>
        /// <returns></returns>
        public bool DescribesPack(string desc, string disp)
        {
            var description = desc.ToLowerInvariant();
            var display = disp.ToLowerInvariant();

            return (display.Contains("pro upgrade") || description.Contains("pro upgrade") ||
                   description.Contains("(pro)") || description.Contains("(upgrade)") ||
                   display.Contains("(pro)") || display.Contains("(upgrade)") ||
                   display.Contains("album") || description.Contains("album") ||
                   display.Contains("export") || description.Contains("export")) && 
                   !description.Contains("depacked") && !display.Contains("depacked");
        }

        /// <summary>
        /// Returns cleaned string for file names, etc
        /// </summary>
        /// <param name="raw_string">Raw string from the songs.dta file</param>
        /// <param name="removeDash">Whether to remove dashes from the string</param>
        /// <param name="DashForSlash">Whether to replace slashes with dashes</param>
        /// <returns></returns>
        public string CleanString(string raw_string, bool removeDash, bool DashForSlash = false)
        {
            var mystring = raw_string;

            //remove forbidden characters if present
            mystring = mystring.Replace("\"", "");
            mystring = mystring.Replace(">", " ");
            mystring = mystring.Replace("<", " ");
            mystring = mystring.Replace(":", " ");
            mystring = mystring.Replace("|", " ");
            mystring = mystring.Replace("?", " ");
            mystring = mystring.Replace("*", " ");
            mystring = mystring.Replace("&#8217;", "'"); //Don't Speak
            mystring = mystring.Replace("   ", " ");
            mystring = mystring.Replace("  ", " ");
            mystring = FixBadChars(mystring).Trim();

            if (removeDash)
            {
                if (mystring.Substring(0, 1) == "-") //if starts with -
                {
                    mystring = mystring.Substring(1, mystring.Length - 1);
                }
                if (mystring.Substring(mystring.Length - 1, 1) == "-") //if ends with -
                {
                    mystring = mystring.Substring(0, mystring.Length - 1);
                }

                mystring = mystring.Trim();
            }

            if (mystring.EndsWith(".", StringComparison.Ordinal)) //can't have files like Y.M.C.A.
            {
                mystring = mystring.Substring(0, mystring.Length - 1);
            }

            mystring = mystring.Replace("\\", DashForSlash && mystring != "AC/DC" ? "-" : (mystring != "AC/DC" ? " " : ""));
            mystring = mystring.Replace("/", DashForSlash && mystring != "AC/DC" ? "-" : (mystring != "AC/DC" ? " " : ""));

            return mystring;
        }
        
        /// <summary>
        /// Exports log to text file
        /// </summary>
        /// <param name="FormName">Name of the form calling the function, names the log tex file</param>
        /// <param name="logItems">Items in the lstLog listbox</param>
        public void ExportLog(string FormName, ListBox.ObjectCollection logItems)
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var sfd = new SaveFileDialog
            {
                InitialDirectory = desktop,
                Title = "Export log file",
                FileName = FormName.Replace(" ", "").Replace("*", "") + "_log",
                Filter = "Text File (*.txt)|*.txt",
                OverwritePrompt = true
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            var sw = new StreamWriter(sfd.FileName, false, Encoding.Default);

            foreach (var line in logItems)
            {
                sw.WriteLine(line);
            }
            sw.Dispose();
        }

        /// <summary>
        /// Creates RAR archive with highest compression setting
        /// </summary>
        /// <param name="rar_path">Full path to the RAR.exe file</param>
        /// <param name="archive_name">Name for the RAR archive to be created</param>
        /// <param name="arguments">Arguments required by RAR.exe</param>
        /// <returns></returns>
        public bool CreateRAR(string rar_path, string archive_name, string arguments)
        {
            try
            {
                DeleteFile(archive_name);             

                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = rar_path,
                    Arguments = arguments,
                    WorkingDirectory = Application.StartupPath + "\\bin\\"
                };
                var process = Process.Start(startInfo);
                do
                {
                    //wait
                } while (!process.HasExited);
                process.Dispose();

                if (File.Exists(archive_name))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
        #endregion

        #region MIDI Stuff

        /// <summary>
        /// Returns clean Track Name from midi event string
        /// </summary>
        /// <param name="raw_event">The raw ToString value of the midi event</param>
        /// <returns></returns>
        public string GetMidiTrackName(string raw_event)
        {
            var name = raw_event;
            name = name.Substring(2, name.Length - 2); //remove track number
            name = name.Replace("SequenceTrackName", "");
            return name.Trim();
        }

        /// <summary>
        /// Returns whether the midi has EMH charted by counting and comparing number of notes
        /// </summary>
        /// <param name="track">Midi track to process (i.e. drums, keys)</param>
        /// <param name="ExpertLow">Lower value cutoff for Expert Difficulty</param>
        /// <param name="ExpertHigh">Higher value cutoff for Expert Difficulty</param>
        /// <param name="HardLow">Lower value cutoff for Hard Difficulty</param>
        /// <param name="HardHigh">Higher value cutoff for Hard Difficulty</param>
        /// <param name="MediumLow">Lower value cutoff for Medium Difficulty</param>
        /// <param name="MediumHigh">Higher value cutoff for Medium Difficulty</param>
        /// <param name="EasyLow">Lower value cutoff for Easy Difficulty</param>
        /// <param name="EasyHigh">Higher value cutoff for Easy Difficulty</param>
        /// <param name="OnlyCheckForEMH">True if you only care whether EMH has ANYTHING charted</param>
        /// <returns></returns>
        private int CheckTrackForEMH(IList<MidiEvent> track, int ExpertLow, int ExpertHigh, int HardLow, int HardHigh, int MediumLow, int MediumHigh, int EasyLow, int EasyHigh, bool OnlyCheckForEMH = false)
        {
            var Expert = new List<MidiEvent>();
            var Hard = new List<MidiEvent>();
            var Medium = new List<MidiEvent>();
            var Easy = new List<MidiEvent>();
            var trackname = GetMidiTrackName(track[0].ToString());
            var x_only = 0;

            try
            {
                foreach (var notes in track)
                {
                    if (notes.CommandCode != MidiCommandCode.NoteOn) continue;
                    var note = (NoteOnEvent)notes;
                    if (note.NoteNumber >= ExpertLow && note.NoteNumber <= ExpertHigh)
                    {
                        Expert.Add(notes);
                    }
                    else if (note.NoteNumber >= HardLow && note.NoteNumber <= HardHigh)
                    {
                        Hard.Add(notes);
                    }
                    else if (note.NoteNumber >= MediumLow && note.NoteNumber <= MediumHigh)
                    {
                        Medium.Add(notes);
                    }
                    else if (note.NoteNumber >= EasyLow && note.NoteNumber <= EasyHigh)
                    {
                        Easy.Add(notes);
                    }
                }

                if (Hard.Count() >= Expert.Count() && !OnlyCheckForEMH)
                {
                    MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: " + trackname + " Hard has " + (!Hard.Any() ? "0 notes" : "the same amount or more notes than Expert");
                    x_only++;
                }
                else if (Hard.Count() < 10 && Expert.Count() > 10)
                {
                    MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: " + trackname + " Hard " + (Hard.Any() ? "only has " + Hard.Count() + " notes but Expert has " + Expert.Count + " notes" : "has 0 notes");
                    x_only++;
                }
                if (Medium.Count() >= Hard.Count() && !OnlyCheckForEMH)
                {
                    MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: " + trackname + " Medium has " + (!Medium.Any() ? "0 notes" : "the same amount or more notes than Hard");
                    x_only++;
                }
                else if (Medium.Count() < 10 && Expert.Count() > 10)
                {
                    MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: " + trackname + " Medium " + (Medium.Any() ? "only has " + Medium.Count() + " notes but Expert has " + Expert.Count + " notes" : "has 0 notes");
                    x_only++;
                }
                if (Easy.Count() >= Medium.Count() && !OnlyCheckForEMH)
                {
                    MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: " + trackname + " Easy has " + (!Easy.Any() ? "0 notes" : "the same amount or more notes than Medium");
                    x_only++;
                }
                else if (Easy.Count() < 10 && Expert.Count() > 10)
                {
                    MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: " + trackname + " Easy " + (Easy.Any() ? "only has " + Easy.Count() + " notes but Expert has " + Expert.Count + " notes" : "has 0 notes");
                    x_only++;
                }
            }
            catch (Exception)
            {
                MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nERROR reading MIDI file to check if " + trackname + " has lower difficulties";
            }

            return x_only;
        }

        public MidiFile NemoLoadMIDI(string midi_in)
        {
            //NAudio is limited in its ability to read some non-standard MIDIs
            //before this step was added, 3 different errors would prevent this program from reading
            //MIDIs with those situations
            //thanks raynebc we can fix them first and load the fixed MIDIs
            var midishrink = Application.StartupPath + "\\bin\\midishrink.exe";
            if (!File.Exists(midishrink)) return null;
            var midi_out = Application.StartupPath + "\\bin\\temp.mid";
            DeleteFile(midi_out);
            MidiFile MIDI;
            try
            {
                MIDI = new MidiFile(midi_in, false);
            }
            catch (Exception)
            {
                var folder = Path.GetDirectoryName(midi_in) ?? Environment.CurrentDirectory;
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    FileName = Application.StartupPath + "\\bin\\midishrink.exe",
                    Arguments = "\"" + midi_in + "\" \"" + midi_out + "\"",
                    WorkingDirectory = folder
                };
                var start = (DateTime.Now.Minute * 60) + DateTime.Now.Second;
                var process = Process.Start(startInfo);
                do
                {
                    //this code checks for possible memory leak in midishrink
                    //typical usage outputs a fixed file in 1 second or less, at 15 seconds there's a problem
                    if ((DateTime.Now.Minute * 60) + DateTime.Now.Second - start < 15) continue;
                    break;

                } while (!process.HasExited);
                if (!process.HasExited)
                {
                    process.Kill();
                    process.Dispose();
                }
                if (File.Exists(midi_out))
                {
                    try
                    {
                        MIDI = new MidiFile(midi_out, false);
                    }
                    catch (Exception)
                    {
                        MIDI = null;
                    }
                }
                else
                {
                    MIDI = null;
                }
            }
            DeleteFile(midi_out);  //the file created in the loop is useless, delete it
            return MIDI;
        }

        public bool DoesMidiHaveEMH(string midifile, bool pro_keys_enabled = true)
        {
            MIDI_ERROR_MESSAGE = "Starting Basic EMH Check";
            var Expert = new List<MidiEvent>();
            var Hard = new List<MidiEvent>();
            var Medium = new List<MidiEvent>();
            var Easy = new List<MidiEvent>();
            var x_only = 0;

            try
            {
                var songMidi = NemoLoadMIDI(midifile);
                if (songMidi == null)
                {
                    MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nERROR: Could not load MIDI file to check if charts have lower difficulties";
                    return false;
                }
                for (var i = 0; i < songMidi.Events.Tracks; i++)
                {
                    if (songMidi.Events[i][0].ToString().ToLowerInvariant().Contains("part drums"))
                    {
                        x_only = x_only + CheckTrackForEMH(songMidi.Events[i], 96, 100, 84, 88, 72, 76, 60, 64);
                    }
                    else if (songMidi.Events[i][0].ToString().ToLowerInvariant().Contains("part bass"))
                    {
                        x_only = x_only + CheckTrackForEMH(songMidi.Events[i], 96, 100, 84, 88, 72, 76, 60, 64);
                    }
                    else if (songMidi.Events[i][0].ToString().ToLowerInvariant().Contains("part guitar"))
                    {
                        x_only = x_only + CheckTrackForEMH(songMidi.Events[i], 96, 100, 84, 88, 72, 76, 60, 64);
                    }
                    else if (songMidi.Events[i][0].ToString().ToLowerInvariant().Contains("part keys") && !songMidi.Events[i][0].ToString().ToLowerInvariant().Contains("anim"))
                    {
                        x_only = x_only + CheckTrackForEMH(songMidi.Events[i], 96, 100, 84, 88, 72, 76, 60, 64);
                    }
                    else if (songMidi.Events[i][0].ToString().ToLowerInvariant().Contains("real_bass"))
                    {
                        x_only = x_only + CheckTrackForEMH(songMidi.Events[i], 96, 99, 72, 75, 48, 51, 24, 27);
                    }
                    else if (songMidi.Events[i][0].ToString().ToLowerInvariant().Contains("real_guitar"))
                    {
                        x_only = x_only + CheckTrackForEMH(songMidi.Events[i], 96, 101, 72, 77, 48, 53, 24, 29);
                    }
                    else if (songMidi.Events[i][0].ToString().ToLowerInvariant().Contains("real_keys") && pro_keys_enabled)
                    {
                        var track = GetMidiTrackName(songMidi.Events[i][0].ToString());
                        foreach (var notes in from notes in songMidi.Events[i] where notes.CommandCode == MidiCommandCode.NoteOn let note = (NoteOnEvent)notes where note.NoteNumber >= 48 && note.NoteNumber <= 72 select notes)
                        {
                            if (track.ToLowerInvariant().Contains("keys_x"))
                            {
                                Expert.Add(notes);
                            }
                            else if (track.ToLowerInvariant().Contains("keys_h"))
                            {
                                Hard.Add(notes);
                            }
                            else if (track.ToLowerInvariant().Contains("keys_m"))
                            {
                                Medium.Add(notes);
                            }
                            else if (track.ToLowerInvariant().Contains("keys_e"))
                            {
                                Easy.Add(notes);
                            }
                        }
                    }
                }

                if (Expert.Any())
                {
                    if (Hard.Count() >= Expert.Count())
                    {
                        MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: PART REAL_KEYS_H has " +
                                             (!Hard.Any() ? "0 notes" : "the same amount or more notes than Expert");
                        x_only++;
                    }
                    else if (Hard.Count() < 10 && Expert.Count() > 10)
                    {
                        MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: PART REAL_KEYS_H " +
                                             (Hard.Any() ? "only has " + Hard.Count() + " notes but Expert has " + Expert.Count +
                                                    " notes" : "has 0 notes");
                        x_only++;
                    }
                    if (Medium.Count() >= Hard.Count())
                    {
                        MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: PART REAL_KEYS_M has " +
                                             (!Medium.Any() ? "0 notes" : "the same amount or more notes than Hard");
                        x_only++;
                    }
                    else if (Medium.Count() < 10 && Expert.Count() > 10)
                    {
                        MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: PART REAL_KEYS_M " +
                                             (Medium.Any() ? "only has " + Medium.Count() + " notes but Expert has " +
                                                    Expert.Count + " notes" : "has 0 notes");
                        x_only++;
                    }
                    if (Easy.Count() >= Medium.Count())
                    {
                        MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: PART REAL_KEYS_E has " +
                                             (!Easy.Any() ? "0 notes" : "the same amount or more notes than Medium");
                        x_only++;
                    }
                    else if (Easy.Count() < 10 && Expert.Count() > 10)
                    {
                        MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nWARNING: PART REAL_KEYS_E " +
                                             (Easy.Any() ? "only has " + Easy.Count() + " notes but Expert has " + Expert.Count +
                                                    " notes" : "has 0 notes");
                        x_only++;
                    }
                }
            }
            catch (Exception)
            {
                MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nERROR: Could not load MIDI file to check if charts have lower difficulties";
                return false;
            }

            if (x_only == 0)
            {
                MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nMIDI passed Basic EMH Check without reporting any problems\nThis means the charts most likely have reductions charted";
                return true;
            }
            if (x_only > 0 && x_only < 4)
            {
                MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nThere " + (x_only == 1 ? "was" : "were") + " only " + x_only + (x_only == 1 ? " problem" : " problems") + " reported\nThis means the charts most likely have reductions charted except where reported in the log";
                return true;
            }
            MIDI_ERROR_MESSAGE = MIDI_ERROR_MESSAGE + "\nMIDI failed Basic EMH Check. This means the charts are most likely expert only\nRefer to the log to see which chart(s) reported problems";
            return false;
        }
        #endregion

        #region Savegame File Stuff

        public bool ReplaceSaveImages(string savefile, string image_folder, bool ps3 = false)
        {
            if (!File.Exists(savefile)) return false;

            var datfile = savefile;
            var tempdat = Application.StartupPath + "\\bin\\temp.dat";
            var isCON = VariousFunctions.ReadFileType(savefile) == XboxFileType.STFS;
            if (isCON)
            {
                ps3 = false;

                var package = new STFSPackage(savefile);
                if (!package.ParseSuccess)
                {
                    package.CloseIO();
                    return false;
                }
                var xent = package.GetFile("save.dat");
                if (xent == null)
                {
                    package.CloseIO();
                    return false;
                }
                DeleteFile(tempdat);
                if (!xent.ExtractToFile(tempdat))
                {
                    package.CloseIO();
                    return false;
                }
                datfile = tempdat;
                package.CloseIO();
            }
            
            var in_char = image_folder + "character_";
            var in_art = image_folder + "art_";
            var ext = ps3 ? ".png_ps3" : ".png_xbox";
            const int PS3_OFFSET = 0x74;

            try
            {
                //write band name
                const int BAND_OFFSET = 0x43A773;
                const int BAND_LENGTH = 31;
                const string BAND_BLANK = "                              ";
                var blength = SaveFileBandName.Length > BAND_LENGTH ? BAND_LENGTH : SaveFileBandName.Length;
                var bname = (SaveFileBandName + BAND_BLANK).Substring(0, BAND_LENGTH);
                var bw = new BinaryWriter(File.Open(datfile, FileMode.Open, FileAccess.ReadWrite));
                bw.BaseStream.Seek((ps3 ? BAND_OFFSET + PS3_OFFSET : BAND_OFFSET) - 4, SeekOrigin.Begin);
                bw.Write(Convert.ToByte(blength));
                bw.BaseStream.Seek(ps3 ? BAND_OFFSET + PS3_OFFSET : BAND_OFFSET, SeekOrigin.Begin);
                bw.Write(Encoding.UTF8.GetBytes(bname));
                bw.Dispose();

                //write character names
                var NAME_OFFSET = 0x4B1AB;
                const int NAME_SPACER = 0x20EDE;
                const int NAME_LENGTH = 24;
                const string NAME_BLANK = "                       ";
                foreach (var n in SaveFileCharNames)
                {
                    var name = (n + NAME_BLANK).Substring(0, NAME_LENGTH);
                    var nlength = n.Length > NAME_LENGTH ? NAME_LENGTH : n.Length;
                    bw = new BinaryWriter(File.Open(datfile, FileMode.Open, FileAccess.ReadWrite));
                    bw.BaseStream.Seek((ps3 ? NAME_OFFSET + PS3_OFFSET : NAME_OFFSET) - 4, SeekOrigin.Begin);
                    bw.Write(Convert.ToByte(nlength));
                    bw.BaseStream.Seek(ps3 ? NAME_OFFSET + PS3_OFFSET : NAME_OFFSET, SeekOrigin.Begin);
                    bw.Write(Encoding.UTF8.GetBytes(name));
                    bw.Dispose();

                    NAME_OFFSET += NAME_LENGTH + NAME_SPACER;
                }
                
                //write all art images
                var ART_OFFSET = 0x19495B;
                const int ART_SIZE = 0x10020;
                const int ART_SPACER = 0x214;
                for (var i = 0; i < 19; i++)
                {
                    var file = in_art + (i + 1) + ext;
                    if (!File.Exists(file))
                    {
                        ART_OFFSET += ART_SIZE + ART_SPACER;
                        continue;
                    }

                    var img = File.ReadAllBytes(file);
                    bw = new BinaryWriter(File.Open(datfile, FileMode.Open, FileAccess.ReadWrite));
                    bw.BaseStream.Seek(ps3 ? ART_OFFSET + PS3_OFFSET : ART_OFFSET, SeekOrigin.Begin);
                    bw.Write(img, 0, ART_SIZE);
                    bw.Dispose();

                    ART_OFFSET += ART_SIZE + ART_SPACER;
                }

                //write all character images
                var CHAR_OFFSET = 0x4C07D;
                const int CHAR_SPACER = 0xED6;
                const int CHAR_SIZE = 0x20020;
                for (var i = 0; i < 19; i++)
                {
                    var file = in_char + (i + 1) + ext;
                    if (!File.Exists(file))
                    {
                        CHAR_OFFSET += CHAR_SIZE + CHAR_SPACER;
                        continue;
                    }

                    var img = File.ReadAllBytes(file);
                    bw = new BinaryWriter(File.Open(datfile, FileMode.Open, FileAccess.ReadWrite));
                    bw.BaseStream.Seek(ps3 ? CHAR_OFFSET + PS3_OFFSET : CHAR_OFFSET, SeekOrigin.Begin);
                    bw.Write(img, 0, CHAR_SIZE);
                    bw.Dispose();

                    CHAR_OFFSET += CHAR_SIZE + CHAR_SPACER;
                }

                if (!isCON) return true;
                if (!File.Exists(tempdat)) return false;
                var package = new STFSPackage(savefile);
                if (!package.ParseSuccess)
                {
                    package.CloseIO();
                    return false;
                }
                var xent = package.GetFile("save.dat");
                if (xent == null)
                {
                    package.CloseIO();
                    return false;
                }
                if (!xent.Replace(tempdat))
                {
                    package.CloseIO();
                    return false;
                }
                package.CloseIO();
                DeleteFile(tempdat);
                SignCON(savefile);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving changes to save game file\n" + ex.Message, "Nemo Tools", MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }
        }
        
        public bool ExtractSaveImages(string savefile, string savepath, bool ps3 = false)
        {
            if (!File.Exists(savefile)) return false;
            
            var savebytes = File.ReadAllBytes(savefile);
            SaveFileCharNames = new List<string>();

            var out_folder = savepath.Replace(".dat", "") + "_extracted\\";
            if (!Directory.Exists(out_folder))
            {
                Directory.CreateDirectory(out_folder);
            }
            
            try
            {
                const int BAND_OFFSET = 0x43A773;
                const int BAND_LENGTH = 31;
                const int PS3_OFFSET = 0x74;
                var ext = ps3 ? ".png_ps3" : ".png_xbox";

                //get band name from save file
                SaveFileBandName = "";
                var name_stream = new MemoryStream(savebytes, ps3 ? BAND_OFFSET + PS3_OFFSET : BAND_OFFSET, BAND_LENGTH);
                var name_bytes = new byte[BAND_LENGTH];
                name_stream.Read(name_bytes, 0, BAND_LENGTH);
                name_stream.Dispose();
                SaveFileBandName = Encoding.UTF8.GetString(name_bytes).Replace("\0", "").Trim();

                //get character names
                var NAME_OFFSET = 0x4B1AB;
                const int NAME_SPACER = 0x20EDE;
                const int NAME_LENGTH = 24;
                for (var i = 0; i < 10; i++)
                {
                    name_stream = new MemoryStream(savebytes, ps3 ? NAME_OFFSET + PS3_OFFSET : NAME_OFFSET, NAME_LENGTH);
                    name_bytes = new byte[NAME_LENGTH];
                    name_stream.Read(name_bytes, 0, NAME_LENGTH);
                    name_stream.Dispose();
                    var name = Encoding.UTF8.GetString(name_bytes).Replace("\0", "").Trim();

                    NAME_OFFSET += NAME_LENGTH + NAME_SPACER;
                    if (string.IsNullOrWhiteSpace(name)) break;
                    SaveFileCharNames.Add(name);
                }
                
                //grab character images
                var CHAR_OFFSET = 0x4C07D;
                const int CHAR_SIZE = 0x20020;
                const int CHAR_SPACER = 0xED6;
                var CHAR_HEADER = new byte[]
                {
                    0x01, 0x08, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00
                };
                var CHAR_COUNTER = 0;
                var OUTPUT_CHAR = out_folder + "character_";
                for (var i = 0; i < 10; i++)
                {
                    var save_stream = new MemoryStream(savebytes, ps3 ? CHAR_OFFSET + PS3_OFFSET : CHAR_OFFSET, CHAR_HEADER.Length);
                    var check_bytes = new byte[CHAR_HEADER.Length];
                    save_stream.Read(check_bytes, 0, CHAR_HEADER.Length);
                    save_stream.Dispose();
                    
                    if (!check_bytes.SequenceEqual(CHAR_HEADER))
                    {
                        CHAR_OFFSET += CHAR_SIZE + CHAR_SPACER;
                        continue;
                    }

                    var img_stream = new MemoryStream(savebytes, ps3 ? CHAR_OFFSET + PS3_OFFSET : CHAR_OFFSET, CHAR_SIZE);
                    var img_bytes = new byte[CHAR_SIZE];
                    img_stream.Read(img_bytes, 0, CHAR_SIZE);
                    img_stream.Dispose();

                    CHAR_OFFSET += CHAR_SIZE + CHAR_SPACER;
                    CHAR_COUNTER++;
                    var outfile = OUTPUT_CHAR + CHAR_COUNTER + ext;
                    DeleteFile(outfile);
                    File.WriteAllBytes(outfile, img_bytes);
                    ConvertRBImage(outfile);
                }
                
                //grab art images
                var ART_OFFSET = 0x19495B;
                const int ART_SIZE = 0x10020;
                const int ART_SPACER = 0x214;
                var ART_HEADER = new byte[]
                {
                    0x01, 0x08, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00
                };
                var ART_COUNTER = 0;
                var OUTPUT_ART = out_folder + "art_";

                for (var i = 0; i < 19; i ++)
                {
                    var save_stream = new MemoryStream(savebytes, ps3 ? ART_OFFSET + PS3_OFFSET : ART_OFFSET, ART_HEADER.Length);
                    var check_bytes = new byte[ART_HEADER.Length];
                    save_stream.Read(check_bytes, 0, ART_HEADER.Length);
                    save_stream.Dispose();

                    
                    if (!check_bytes.SequenceEqual(ART_HEADER))
                    {
                        ART_OFFSET += ART_SIZE + ART_SPACER;
                        continue;
                    }

                    var img_stream = new MemoryStream(savebytes, ps3 ? ART_OFFSET + PS3_OFFSET : ART_OFFSET, ART_SIZE);
                    var img_bytes = new byte[ART_SIZE];
                    img_stream.Read(img_bytes, 0, ART_SIZE);
                    img_stream.Dispose();

                    ART_OFFSET += ART_SIZE + ART_SPACER;
                    ART_COUNTER++;
                    var outfile = OUTPUT_ART + ART_COUNTER + ext;
                    DeleteFile(outfile);
                    File.WriteAllBytes(outfile, img_bytes);
                    ConvertRBImage(outfile);
                }

                var success = ART_COUNTER != 0 || CHAR_COUNTER != 0;
                if (success) return true;
                DeleteFolder(out_folder, true);
                MessageBox.Show("No character or art images found in that save game file\nThis tool only allows you to edit existing images,\nso try again after you've created characters or art in game",
                    "Save File Image Editor", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            catch (Exception)
            {
                DeleteFolder(out_folder, true);
                return false;
            }
        }

        public bool ExtractWiiSaveImages(string savefile, string savepath)
        {
            //I won't lie, treating the Wii like a red-headed stepchild here. too crappy to dedicate much more attention
            //maybe you can finesse it?

            if (!File.Exists(savefile)) return false;

            SaveFileBandName = "";
            var savebytes = File.ReadAllBytes(savefile);
            const int CHAR_SIZE = 0x4020;
            const int ART_SIZE = 0x10020;
            var char_header = new byte[]
                {
                    0x01, 0x04, 0x48, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x01, 0x40, 0x00, 0x00, 0x00, 0x00
                };
            var art_header = new byte[]
                {
                    0x01, 0x20, 0x40, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x80, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00
                };

            SaveFileCharNames = new List<string>();
            var out_folder = savepath.Replace(".dat", "") + "_extracted\\";
            if (!Directory.Exists(out_folder))
            {
                Directory.CreateDirectory(out_folder);
            }
            var out_char = out_folder + "character_";
            var char_counter = 0;
            var out_art = out_folder + "art_";
            var art_counter = 0;

            try
            {
                //get band name from save file
                //wii sucks, the offset varies wildly based on which "slot" the game is saved to
                //this will search all four slots for "a" name, but it's impossible to know
                //which is the correct name since we don't know whose slot we're looking at
                SaveFileBandName = "";
                var band_offsets = new List<int> { 0x157BCB, 0x297BCB, 0x3D7BCB, 0x517BCB };
                foreach (var bandOffset in band_offsets)
                {
                    var name_stream = new MemoryStream(savebytes, bandOffset, 24);
                    var name_bytes = new byte[24];
                    name_stream.Read(name_bytes, 0, 24);
                    name_stream.Dispose();
                    SaveFileBandName = Encoding.UTF8.GetString(name_bytes).Replace("\0", "").Trim();

                    //if there's no band name, continue, otherwise stop here
                    if (SaveFileBandName != "") break;
                }

                for (var i = 0; i < savebytes.Length - 16; i++)
                {
                    var save_stream = new MemoryStream(savebytes, i, 16);
                    var check_bytes = new byte[16];
                    save_stream.Read(check_bytes, 0, 16);
                    save_stream.Dispose();

                    if (check_bytes.SequenceEqual(char_header))
                    {
                        char_counter++;

                        //grab character image
                        var img = new MemoryStream(savebytes, i, CHAR_SIZE);
                        var imgbytes = new byte[CHAR_SIZE];
                        img.Read(imgbytes, 0, CHAR_SIZE);
                        img.Dispose();

                        var outfile = out_char + char_counter + ".png_wii";
                        DeleteFile(outfile);
                        File.WriteAllBytes(outfile, imgbytes);
                        ConvertWiiImage(outfile);

                        //grab character name
                        const int name_offset = 0xED2;
                        var name_stream = new MemoryStream(savebytes, i - name_offset, 24);
                        var name_bytes = new byte[24];
                        name_stream.Read(name_bytes, 0, 24);
                        name_stream.Dispose();

                        var name = Encoding.UTF8.GetString(name_bytes).Replace("\0", "").Trim();

                        i = i + CHAR_SIZE;

                        if (string.IsNullOrWhiteSpace(name)) break;
                        SaveFileCharNames.Add(name);
                    }
                    else if (check_bytes.SequenceEqual(art_header))
                    {
                        art_counter++;

                        //grab art image
                        var img = new MemoryStream(savebytes, i, ART_SIZE);
                        var imgbytes = new byte[ART_SIZE];
                        img.Read(imgbytes, 0, ART_SIZE);
                        img.Dispose();

                        var outfile = out_art + art_counter + ".png_wii";
                        DeleteFile(outfile);
                        File.WriteAllBytes(outfile, imgbytes);
                        ConvertWiiImage(outfile);
                        i = i + ART_SIZE;
                    }
                }
                
                var success = char_counter != 0 || art_counter != 0;
                if (!success)
                {
                    DeleteFolder(out_folder, true);
                }
                return success;
            }
            catch (Exception)
            {
                DeleteFolder(out_folder, true);
                return false;
            }
        }
        #endregion

        #region Mogg Stuff
        public void ReleaseStreamHandle()
        {
            try
            {
                PlayingOggStreamHandle.Free();
            }
            catch (Exception)
            {}
        }

        public IntPtr GetOggStreamIntPtr()
        {
            ReleaseStreamHandle();
            PlayingOggStreamHandle = GCHandle.Alloc(PlayingSongOggData, GCHandleType.Pinned);
            return PlayingOggStreamHandle.AddrOfPinnedObject();
        }

        public bool EncM(byte[] mData, string mOut, bool doPS3 = false)
        {
            //REDACTED BY TROJANNEMO
            return false;
        }

        public bool MoggIsEncrypted(byte[] mData)
        {
            //REDACTED BY TROJANNEMO
            return false;
        }

        public bool DecM(byte[] mData, bool bypass = false, bool keep_header = true, DecryptMode mode = DecryptMode.ToFile, string mOut = "")
        {
            //REDACTED BY TROJANNEMO
            return false;
        }

        public void RemoveMHeader(byte[] mData, DecryptMode mode, string mOut)
        {
            byte[] buffer;
            using (var br = new BinaryReader(new MemoryStream(mData)))
            {
                br.ReadInt32();
                var num = br.ReadInt32();
                br.BaseStream.Seek(num, SeekOrigin.Begin);
                buffer = new byte[br.BaseStream.Length - num];
                br.Read(buffer, 0, buffer.Length);
            }
            if (mode == DecryptMode.ToMemory)
            {
                PlayingSongOggData = buffer;
            }
            else
            {
                WriteOutData(buffer, mOut);
            }
        }

        public void WriteOutData(byte[] mData, string mOut)
        {
            DeleteFile(mOut);
            using (var fs = File.Create(mOut))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.Write(mData);
                }
            }
        }

        public bool IsC3Mogg(byte[] mData)
        {
            //REDACTED BY TROJANNEMO
            return false;
        }
        
        public byte[] DeObfM(byte[] mData)
        {
            //REDACTED BY TROJANNEMO
            return mData;
        }

        public byte[] ObfM(byte[] mData)
        {
            //REDACTED BY TROJANNEMO
            return mData;
        }
        
        private GCHandle PlayingOggStreamHandle;
        public byte[] PlayingSongOggData;
        public byte[] NextSongOggData;
        
        #endregion
        
    }

    public enum DecryptMode
    {
        ToFile,
        ToMemory
    }

    public enum CryptVersion
    {
        x0A = 0x0A, //No encryption
        x0B = 0x0B, //RB1, RB1 DLC
        x0C = 0x0C, //RB2, AC/DC Live, some RB2 DLC
        x0E = 0x0E, //Lego, Green Day, most RB2 DLC
        x0F = 0x0F, //RBN
        x10 = 0x10, //RB3, RB3 DLC
    }
}
