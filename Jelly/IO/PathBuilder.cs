using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;

namespace Jelly.IO;

public class PathBuilder
{
    public bool UseDefaultDirSeparator { get; set; } = true;
    public char DirSeparatorOverride { get; set; } = '/';
    public bool AppendFinalSeparator { get; set; } = false;

    public static string LocalAppdataPath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
    public static string AppdataPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify);

    public string Create(params string[] dirs)
    {
        var str = "";

        for (int i = 0; i < dirs.Length; i++)
        {
            string dirString = dirs[i];

            if(i > 0)
                str += UseDefaultDirSeparator ? Path.DirectorySeparatorChar : DirSeparatorOverride;

            str += dirString;
        }

        if(AppendFinalSeparator)
            str += UseDefaultDirSeparator ? Path.DirectorySeparatorChar : DirSeparatorOverride;

        return str;
    }

    public string Create(IEnumerable<string> dirs) => Create(dirs);
}
