<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts"
  exclude-result-prefixes="xsl msxsl user"
  version="1.0">

  <xsl:output method="xml" indent="no" />

  <msxsl:script language="C#" implements-prefix="user">
    <msxsl:assembly name="System.Web" />
    <msxsl:using namespace="System" />
    <msxsl:using namespace="System.Web" />
    <![CDATA[
    public string NormalizeXAPName(string origName)
    {
      return origName.Replace('.','_');
    }
    public string GetRelativePath(string from, string to)
    {
      try
      {
        var current = Environment.CurrentDirectory;
        from = System.IO.Path.Combine(current, from.Replace('\\', '/'));
        to = System.IO.Path.Combine(current, to.Replace('\\', '/'));
        return (new Uri(from).MakeRelativeUri(new Uri(to)))
          .ToString().Replace('/', '\\');
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
    }

    public bool ProjectIsActive(
      string platformString,
      string includePlatformString,
      string excludePlatformString,
      string activePlatform)
    {
      // Choose either <Platforms> or <IncludePlatforms>
      if (string.IsNullOrEmpty(platformString))
      {
        platformString = includePlatformString;
      }

      // If the exclude string is set, then we must check this first.
      if (!string.IsNullOrEmpty(excludePlatformString))
      {
        var excludePlatforms = excludePlatformString.Split(',');
        foreach (var i in excludePlatforms)
        {
          if (i == activePlatform)
          {
            // This platform is excluded.
            return false;
          }
        }
      }

      // If the platform string is empty at this point, then we allow
      // all platforms since there's no whitelist of platforms configured.
      if (string.IsNullOrEmpty(platformString))
      {
        return true;
      }

      // Otherwise ensure the platform is in the include list.
      var platforms = platformString.Split(',');
      foreach (var i in platforms)
      {
        if (i == activePlatform)
        {
          return true;
        }
      }

      return false;
    }

    public bool IsTrue(string text)
    {
      return text.ToLower() == "true";
    }

    public string ReadFile(string path)
    {
      path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
      path = path.Replace('\\', System.IO.Path.DirectorySeparatorChar);

      using (var reader = new System.IO.StreamReader(path))
      {
        return reader.ReadToEnd();
      }
    }

    public bool HasXamarinMac()
    {
      return System.IO.File.Exists("/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/lib/mono/XamMac.dll");
    }

    ]]>
  </msxsl:script>

  <xsl:variable name="assembly_name">
    <xsl:choose>
      <xsl:when test="/Input/Properties/AssemblyName
	        /Platform[@Name=/Input/Generation/Platform]">
        <xsl:value-of select="/Input/Properties/AssemblyName/Platform[@Name=/Input/Generation/Platform]" />
      </xsl:when>
      <xsl:when test="/Input/Properties/AssemblyName">
        <xsl:value-of select="/Input/Properties/AssemblyName" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/Input/Projects/Project[@Name=/Input/Generation/ProjectName]/@Name" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:template name="profile_and_version"
    xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <xsl:choose>
      <xsl:when test="/Input/Properties/FrameworkVersions
                      /Platform[@Name=/Input/Generation/Platform]
                      /Version">
        <TargetFrameworkVersion>
          <xsl:value-of select="/Input/Properties/FrameworkVersions
                                                      /Platform[@Name=/Input/Generation/Platform]
                                                      /Version" />
        </TargetFrameworkVersion>
      </xsl:when>
      <xsl:when test="/Input/Properties/FrameworkVersions/Version">
        <TargetFrameworkVersion>
          <xsl:value-of select="/Input/Properties/FrameworkVersions/Version" />
        </TargetFrameworkVersion>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="/Input/Generation/Platform = 'Android'">
            <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Ouya'">
            <TargetFrameworkVersion>v4.1</TargetFrameworkVersion>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Windows8'">
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
            <TargetFrameworkVersion>v8.0</TargetFrameworkVersion>
            <TargetFrameworkIdentifier>WindowsPhone</TargetFrameworkIdentifier>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7' or /Input/Generation/Platform = 'Windows' or /Input/Generation/Platform = 'XBox360'">
            <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'iOS' or /Input/Generation/Platform = 'PSM'">
          </xsl:when>
          <xsl:otherwise>
            <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="/Input/Properties/FrameworkVersions
                      /Platform[@Name=/Input/Generation/Platform]
                      /Profile">
        <TargetFrameworkProfile>
          <xsl:value-of select="/Input/Properties/FrameworkVersions
                                                      /Platform[@Name=/Input/Generation/Platform]
                                                      /Profile" />
        </TargetFrameworkProfile>
      </xsl:when>
      <xsl:when test="/Input/Properties/FrameworkVersions/Profile">
        <TargetFrameworkProfile>
          <xsl:value-of select="/Input/Properties/FrameworkVersions/Profile" />
        </TargetFrameworkProfile>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7' or /Input/Generation/Platform = 'Windows' or /Input/Generation/Platform = 'XBox360' ">
        <TargetFrameworkProfile>Client</TargetFrameworkProfile>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'Windows8' or /Input/Generation/Platform = 'PSM'">
      </xsl:when>
      <xsl:otherwise>
        <TargetFrameworkProfile></TargetFrameworkProfile>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="/Input/Generation/Platform = 'Windows8'">
      <DefaultLanguage>en-US</DefaultLanguage>
    </xsl:if>
  </xsl:template>

  <xsl:template name="configuration"
    xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <xsl:param name="type" />
    <xsl:param name="debug" />
    <xsl:param name="config" />
    <xsl:param name="platform" />
    <PropertyGroup>
      <xsl:attribute name="Condition">
        <xsl:text> '$(Configuration)|$(Platform)' == '</xsl:text>
        <xsl:value-of select="$config" />
	<xsl:text>|</xsl:text>
	<xsl:value-of select="$platform" />
        <xsl:text>' </xsl:text>
      </xsl:attribute>
    <xsl:choose>
      <xsl:when test="$debug = 'true'">
        <DebugSymbols>true</DebugSymbols>
        <Optimize>false</Optimize>
      </xsl:when>
      <xsl:otherwise>
        <Optimize>true</Optimize>
      </xsl:otherwise>
    </xsl:choose>
    <DebugType>full</DebugType>
    <xsl:variable name="platform_path">
      <xsl:choose>
        <xsl:when test="$type = 'Website'">
          <xsl:text></xsl:text>
        </xsl:when>
        <xsl:when test="user:IsTrue(/Input/Properties/PlatformSpecificOutputFolder)">
          <xsl:value-of select="/Input/Generation/Platform" />
	  <xsl:text>\</xsl:text>
	  <xsl:value-of select="$platform" />
	  <xsl:text>\</xsl:text>
	  <xsl:value-of select="$config" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$config" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <OutputPath><xsl:text>bin\</xsl:text><xsl:copy-of select="$platform_path" /></OutputPath>
    <IntermediateOutputPath><xsl:text>obj\</xsl:text><xsl:copy-of select="$platform_path" /></IntermediateOutputPath>
    <DocumentationFile><xsl:text>bin\</xsl:text><xsl:copy-of select="$platform_path" /><xsl:text>\</xsl:text><xsl:copy-of select="$assembly_name" /><xsl:text>.xml</xsl:text></DocumentationFile>
    <DefineConstants>
      <xsl:if test="$debug = 'true'">
        <xsl:text>DEBUG;</xsl:text>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="/Input/Properties/CustomDefinitions">
          <xsl:for-each select="/Input/Properties/CustomDefinitions/Platform">
            <xsl:if test="/Input/Generation/Platform = ./@Name">
              <xsl:value-of select="." />
            </xsl:if>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="/Input/Generation/Platform = 'Android'">
              <xsl:text>PLATFORM_ANDROID</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'iOS'">
              <xsl:text>PLATFORM_IOS</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'Linux'">
              <xsl:text>PLATFORM_LINUX</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'MacOS'">
              <xsl:text>PLATFORM_MACOS</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'Ouya'">
              <xsl:text>PLATFORM_OUYA</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'PSM'">
              <xsl:text>PLATFORM_PSM</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'Windows'">
              <xsl:text>PLATFORM_WINDOWS</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'WindowsDX'">
              <xsl:text>PLATFORM_WINDOWSDX</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'Windows8'">
              <xsl:text>PLATFORM_WINDOWS8</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'WindowsGL'">
              <xsl:text>PLATFORM_WINDOWSGL</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
              <xsl:text>PLATFORM_WINDOWSPHONE</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'XBox360'">
              <xsl:text>PLATFORM_XBOX360</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7'">
              <xsl:text>PLATFORM_WINDOWSPHONE7</xsl:text>
            </xsl:when>
            <xsl:when test="/Input/Generation/Platform = 'Web'">
              <xsl:text>PLATFORM_WEB</xsl:text>
            </xsl:when>
          </xsl:choose>
          <xsl:text>;</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <xsl:choose>
      <xsl:when test="/Input/Properties/ForceArchitecture">
        <PlatformTarget>
          <xsl:value-of select="/Input/Properties/ForceArchitecture" />
        </PlatformTarget>
      </xsl:when>
    </xsl:choose>
    <!--<xsl:call-template name="profile_and_version" />-->
    <xsl:choose>
      <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
        <xsl:choose>
          <xsl:when test="$debug = 'true'">
            <MonoDroidLinkMode>None</MonoDroidLinkMode>
            <AndroidLinkMode>None</AndroidLinkMode>
          </xsl:when>
          <xsl:otherwise>
            <AndroidUseSharedRuntime>False</AndroidUseSharedRuntime>
            <!--<AndroidLinkMode>SdkOnly</AndroidLinkMode>-->
            <EmbedAssembliesIntoApk>True</EmbedAssembliesIntoApk>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'iOS'">
        <xsl:choose>
          <xsl:when test="$debug = 'true'">
            <CheckForOverflowUnderflow>True</CheckForOverflowUnderflow>
            <AllowUnsafeBlocks>True</AllowUnsafeBlocks>
            <MtouchDebug>True</MtouchDebug>
            <MtouchUseArmv7>false</MtouchUseArmv7>
          </xsl:when>
          <xsl:otherwise>
            <MtouchUseArmv7>false</MtouchUseArmv7>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'MacOS'">
        <EnableCodeSigning>False</EnableCodeSigning>
        <CreatePackage>False</CreatePackage>
        <EnablePackageSigning>False</EnablePackageSigning>
        <xsl:choose>
          <xsl:when test="user:HasXamarinMac()">
            <xsl:choose>
              <xsl:when test="/Input/Properties/IncludeMonoRuntimeOnMac">
                <IncludeMonoRuntime><xsl:value-of select="/Input/Properties/IncludeMonoRuntimeOnMac" /></IncludeMonoRuntime>
	            <xsl:if test="/Input/Properties/MonoMacRuntimeLinkMode">
	              <LinkMode><xsl:value-of select="/Input/Properties/MonoMacRuntimeLinkMode" /></LinkMode>
	            </xsl:if>
              </xsl:when>
              <xsl:otherwise>
                <IncludeMonoRuntime>False</IncludeMonoRuntime>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:when>
          <xsl:otherwise>
            <IncludeMonoRuntime>False</IncludeMonoRuntime>
          </xsl:otherwise>
        </xsl:choose>
        <UseSGen>False</UseSGen>
      </xsl:when>
    </xsl:choose>
    </PropertyGroup>
  </xsl:template>

  <xsl:template match="/">

    <xsl:variable
      name="project"
      select="/Input/Projects/Project[@Name=/Input/Generation/ProjectName]" />

    <Project
      DefaultTargets="Build"
      ToolsVersion="4.0"
      xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

      <xsl:if test="/Input/Generation/Platform = 'Windows8'">
        <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
      </xsl:if>

      <PropertyGroup>
        <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
        <xsl:choose>
          <xsl:when test="/Input/Properties/ForceArchitecture">
            <Platform Condition=" '$(Platform)' == '' ">
              <xsl:value-of select="/Input/Properties/ForceArchitecture" />
            </Platform>
          </xsl:when>
          <xsl:otherwise>
            <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:choose>
          <xsl:when test="/Input/Generation/Platform = 'Windows8'">
            <ProductVersion>8.0.30703</ProductVersion>
          </xsl:when>
          <xsl:otherwise>
            <ProductVersion>10.0.0</ProductVersion>
          </xsl:otherwise>
        </xsl:choose>
        <SchemaVersion>2.0</SchemaVersion>
        <ProjectGuid>{<xsl:value-of select="$project/@Guid" />}</ProjectGuid>
        <xsl:choose>
          <xsl:when test="$project/@Type = 'Website'">
            <ProjectTypeGuids>
              <xsl:text>{349C5851-65DF-11DA-9384-00065B846F21};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
            <ProjectTypeGuids>
              <xsl:text>{EFBA0AD7-5A72-4C68-AF49-83D382785DCF};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'iOS'">
            <ProjectTypeGuids>
              <xsl:text>{6BC8ED88-2882-458C-8E55-DFD12B67127B};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'MacOS'">
            <ProjectTypeGuids>
              <xsl:choose>
                <xsl:when test="user:HasXamarinMac()">
                  <xsl:text>{42C0BBD9-55CE-4FC1-8D90-A7348ABAFB23};</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>{948B3504-5B70-4649-8FE4-BDE1FB46EC69};</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'PSM'">
            <ProjectTypeGuids>
              <xsl:text>{69878862-DA7D-4DC6-B0A1-50D8FAB4242F};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Windows8'">
            <ProjectTypeGuids>
              <xsl:text>{BC8A1FFA-BEE3-4634-8014-F334798102B3};</xsl:text>
              <xsl:text>{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
            <ProjectTypeGuids>
              <xsl:text>{C089C8C0-30E0-4E22-80C0-CE093F111A43};</xsl:text>
              <xsl:text>{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</xsl:text>
            </ProjectTypeGuids>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7'">
            <ProjectTypeGuids>
              <xsl:text>{6D335F3A-9D43-41b4-9D22-F6F17C4BE596};</xsl:text>
              <xsl:text>{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</xsl:text>
            </ProjectTypeGuids>
            <XnaFrameworkVersion>v4.0</XnaFrameworkVersion>
            <XnaPlatform>Windows Phone</XnaPlatform>
            <XnaProfile>Reach</XnaProfile>
            <XnaCrossPlatformGroupID>c33a6796-aae0-4084-8878-f02e8c8da387</XnaCrossPlatformGroupID>
            <XnaOutputType>Library</XnaOutputType>
            <XnaRefreshLevel>1</XnaRefreshLevel>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'XBox360'">
            <ProjectTypeGuids>
              <xsl:text>{6D335F3A-9D43-41b4-9D22-F6F17C4BE596};</xsl:text>
              <xsl:text>{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</xsl:text>
            </ProjectTypeGuids>
            <XnaFrameworkVersion>v4.0</XnaFrameworkVersion>
            <XnaPlatform>XBox 360</XnaPlatform>
            <XnaProfile>HiDef</XnaProfile>
            <XnaCrossPlatformGroupID>f651ca56-7702-4520-ae42-e26a4dbb705e</XnaCrossPlatformGroupID>
            <XnaOutputType>Library</XnaOutputType>
            <XnaRefreshLevel>1</XnaRefreshLevel>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'Windows'">
            <ProjectTypeGuids>
              <xsl:text>{6D335F3A-9D43-41b4-9D22-F6F17C4BE596};</xsl:text>
              <xsl:text>{fae04ec0-301f-11d3-bf4b-00c04f79efbc}</xsl:text>
            </ProjectTypeGuids>
            <XnaFrameworkVersion>v4.0</XnaFrameworkVersion>
            <XnaPlatform>Windows</XnaPlatform>
            <XnaProfile>Reach</XnaProfile>
            <XnaCrossPlatformGroupID>f651ca56-7702-4520-ae42-e26a4dbb705e</XnaCrossPlatformGroupID>
            <XnaOutputType>Library</XnaOutputType>
            <XnaRefreshLevel>1</XnaRefreshLevel>
          </xsl:when>
          <xsl:otherwise>
          </xsl:otherwise>
        </xsl:choose>
        <OutputType>
          <xsl:choose>
            <xsl:when test="$project/@Type = 'XNA'">
              <xsl:choose>
                <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Windows8'">
                  <xsl:text>AppContainerExe</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Windows'">
                  <xsl:text>WinExe</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'iOS'">
                  <xsl:text>Exe</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Exe</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:when test="$project/@Type = 'Console'">
              <xsl:text>Exe</xsl:text>
            </xsl:when>
            <xsl:when test="$project/@Type = 'GUI'">
              <xsl:text>WinExe</xsl:text>
            </xsl:when>
            <xsl:when test="$project/@Type = 'GTK'">
              <xsl:text>WinExe</xsl:text>
            </xsl:when>
            <xsl:when test="$project/@Type = 'App'">
              <xsl:choose>
                <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7'">
                  <xsl:text>Library</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Windows8'">
                  <xsl:text>AppContainerExe</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Windows'">
                  <xsl:text>WinExe</xsl:text>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'iOS'">
                  <xsl:text>Exe</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Exe</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>Library</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </OutputType>
        <RootNamespace>
          <xsl:choose>
            <xsl:when test="/Input/Properties/RootNamespace">
              <xsl:value-of select="/Input/Properties/RootNamespace" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$project/@Name" />
            </xsl:otherwise>
          </xsl:choose>
        </RootNamespace>
        <AssemblyName><xsl:copy-of select="$assembly_name" /></AssemblyName>
        <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
	<NoWarn><xsl:value-of select="/Input/Properties/NoWarn" /></NoWarn>
        <xsl:call-template name="profile_and_version" />
        <xsl:choose>
          <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
            <FileAlignment>512</FileAlignment>
            <AndroidSupportedAbis>armeabi,armeabi-v7a,x86</AndroidSupportedAbis>
            <AndroidStoreUncompressedFileExtensions />
            <MandroidI18n />
            <xsl:choose>
              <xsl:when test="Input/Properties/ManifestPrefix">
                <AndroidManifest>
                  <xsl:value-of select="concat(
                                '..\',
                                $project/@Name,
                                '.',
                                /Input/Generation/Platform,
                                '\Properties\AndroidManifest.xml')"/>
                </AndroidManifest>
              </xsl:when>
              <xsl:otherwise>
                <AndroidManifest>Properties\AndroidManifest.xml</AndroidManifest>
              </xsl:otherwise>
            </xsl:choose>
            <DeployExternal>False</DeployExternal>
            <xsl:if test="$project/@Type = 'App'">
              <AndroidApplication>True</AndroidApplication>
              <AndroidResgenFile>Resources\Resource.designer.cs</AndroidResgenFile>
              <AndroidResgenClass>Resource</AndroidResgenClass>
            </xsl:if>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'iOS'">
            <SynchReleaseVersion>False</SynchReleaseVersion>
            <xsl:choose>
              <xsl:when test="$project/@Type = 'App'">
                <ConsolePause>false</ConsolePause>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'MacOS'">
            <xsl:if test="user:HasXamarinMac() = false()">
              <SuppressXamMacUpsell>True</SuppressXamMacUpsell>
            </xsl:if>
          </xsl:when>
          <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
            <xsl:choose>
              <xsl:when test="$project/@Type = 'App'">
                <SilverlightVersion>$(TargetFrameworkVersion)</SilverlightVersion>
                <SilverlightApplication>true</SilverlightApplication>
                <XapFilename>
                  <xsl:value-of select="concat( user:NormalizeXAPName(
                                concat($project/@Name ,'_$(Configuration)','_$(Platform)')),'.xap'
                                )"/>
                </XapFilename>
                <XapOutputs>true</XapOutputs>
                <GenerateSilverlightManifest>true</GenerateSilverlightManifest>
                <xsl:choose>
                  <xsl:when test="Input/Properties/ManifestPrefix">
                    <SilverlightManifestTemplate>
                      <xsl:value-of select="concat(
                        '..\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '\Properties\AppManifest.xml')"/>
                    </SilverlightManifestTemplate>
                  </xsl:when>
                  <xsl:otherwise>
                    <SilverlightManifestTemplate>Properties\AppManifest.xml</SilverlightManifestTemplate>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </PropertyGroup>
      <xsl:choose>
        <xsl:when test="/Input/Generation/Platform = 'iOS'">
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">iPhone</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">iPhone</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">iPhoneSimulator</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">iPhoneSimulator</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Ad-Hoc</xsl:with-param>
            <xsl:with-param name="platform">iPhone</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">AppStore</xsl:with-param>
            <xsl:with-param name="platform">iPhone</xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">x86</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">x86</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">ARM</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">ARM</xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">true</xsl:with-param>
            <xsl:with-param name="config">Debug</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="configuration">
            <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
            <xsl:with-param name="debug">false</xsl:with-param>
            <xsl:with-param name="config">Release</xsl:with-param>
            <xsl:with-param name="platform">AnyCPU</xsl:with-param>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="/Input/Properties/ForceArchitecture">
        <xsl:call-template name="configuration">
          <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
          <xsl:with-param name="debug">true</xsl:with-param>
          <xsl:with-param name="config">Debug</xsl:with-param>
          <xsl:with-param name="platform"><xsl:value-of select="/Input/Properties/ForceArchitecture" /></xsl:with-param>
        </xsl:call-template>
        <xsl:call-template name="configuration">
          <xsl:with-param name="type"><xsl:value-of select="$project/@Type" /></xsl:with-param>
          <xsl:with-param name="debug">false</xsl:with-param>
          <xsl:with-param name="config">Release</xsl:with-param>
          <xsl:with-param name="platform"><xsl:value-of select="/Input/Properties/ForceArchitecture" /></xsl:with-param>
        </xsl:call-template>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="$project/@Type = 'Website'">
          <Import>
            <xsl:attribute name="Project">
              <xsl:text>$(MSBuildExtensionsPath)\Microsoft\</xsl:text>
              <xsl:text>VisualStudio\v10.0\WebApplications\</xsl:text>
              <xsl:text>Microsoft.WebApplication.targets</xsl:text>
            </xsl:attribute>
          </Import>
          <xsl:if test="/Input/Properties/RazorGeneratorTargetsPath != ''">
            <Import>
              <xsl:attribute name="Project">
                <xsl:value-of select="/Input/Properties/RazorGeneratorTargetsPath" />
              </xsl:attribute>
            </Import>
            <Target Name="BeforeBuild">
              <CallTarget Targets="PrecompileRazorFiles" />
            </Target>
          </xsl:if>
        </xsl:when>
      </xsl:choose>

      <xsl:if test="/Input/Generation/Platform = 'WindowsPhone7' or /Input/Generation/Platform = 'XBox360'">
        <ItemGroup>
          <BootstrapperPackage Include=".NETFramework,Version=v4.0,Profile=Client">
            <Visible>False</Visible>
            <ProductName>Microsoft .NET Framework 4 Client Profile %28x86 and x64%29</ProductName>
            <Install>true</Install>
          </BootstrapperPackage>
          <BootstrapperPackage Include="Microsoft.Net.Client.3.5">
            <Visible>False</Visible>
            <ProductName>.NET Framework 3.5 SP1 Client Profile</ProductName>
            <Install>false</Install>
          </BootstrapperPackage>
          <BootstrapperPackage Include="Microsoft.Net.Framework.3.5.SP1">
            <Visible>False</Visible>
            <ProductName>.NET Framework 3.5 SP1</ProductName>
            <Install>false</Install>
          </BootstrapperPackage>
          <BootstrapperPackage Include="Microsoft.Windows.Installer.3.1">
            <Visible>False</Visible>
            <ProductName>Windows Installer 3.1</ProductName>
            <Install>true</Install>
          </BootstrapperPackage>
          <BootstrapperPackage Include="Microsoft.Xna.Framework.4.0">
            <Visible>False</Visible>
            <ProductName>Microsoft XNA Framework Redistributable 4.0</ProductName>
            <Install>true</Install>
          </BootstrapperPackage>
        </ItemGroup>
      </xsl:if>
      
      <ItemGroup>
        <xsl:if test="$project/@Type = 'GTK'">
          <Reference Include="gtk-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="gdk-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="glib-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="glade-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="pango-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
          <Reference Include="atk-sharp, Version=2.4.0.0, Culture=neutral, PublicKeyToken=35e10195dab3c99f">
            <SpecificVersion>False</SpecificVersion>
          </Reference>
        </xsl:if>

        <xsl:if test="/Input/Generation/Platform = 'MacOS'">
          <xsl:choose>
            <xsl:when test="user:HasXamarinMac()">
              <Reference Include="XamMac" />
            </xsl:when>
            <xsl:otherwise>
              <Reference Include="MonoMac" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-path" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-path]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-path]) = 0">
              <xsl:if test="
                count(/Input/Projects/ContentProject[@Name=$include-path]) = 0">

                <Reference>
                  <xsl:attribute name="Include">
                    <xsl:value-of select="@Include" />
                  </xsl:attribute>
                  <xsl:text />
                </Reference>
              </xsl:if>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-name" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-name]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-name]) > 0">

              <xsl:variable name="extern"
                select="/Input/Projects/ExternalProject[@Name=$include-name]" />

              <xsl:for-each select="$extern/Reference">
                <xsl:variable name="refd-name" select="@Include" />
                <xsl:if test="count(/Input/Projects/Project[@Name=refd-name]) = 0">
                  <Reference>
                    <xsl:attribute name="Include">
                      <xsl:value-of select="@Include" />
                    </xsl:attribute>
                    <xsl:if test="@Aliases != ''">
                      <Aliases><xsl:value-of select="@Aliases" /></Aliases>
                    </xsl:if>
                  </Reference>
                </xsl:if>
              </xsl:for-each>
              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./Reference">
                  <xsl:variable name="refd-name" select="@Include" />
                  <xsl:if test="count(/Input/Projects/Project[@Name=refd-name]) = 0">
                    <Reference>
                      <xsl:attribute name="Include">
                        <xsl:value-of select="@Include" />
                      </xsl:attribute>
                      <xsl:if test="@Aliases != ''">
                        <Aliases><xsl:value-of select="@Aliases" /></Aliases>
                      </xsl:if>
                    </Reference>
                  </xsl:if>
                </xsl:for-each>
              </xsl:for-each>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>

        <xsl:if test="/Input/Generation/Platform = 'Web'">
          <Reference>
            <xsl:attribute name="Include">
              <xsl:text>JSIL.Meta</xsl:text>
            </xsl:attribute>
            <HintPath>
              <xsl:value-of select="/Input/Generation/JSILDirectory" />
              <xsl:if test="/Input/Generation/HostPlatform = 'Linux' or /Input/Generation/HostPlatform = 'MacOS'">
                <xsl:text>/</xsl:text>
              </xsl:if>
              <xsl:if test="/Input/Generation/HostPlatform = 'Windows'">
                <xsl:text>\</xsl:text>
              </xsl:if>
              <xsl:text>JSIL.Meta.dll</xsl:text>
            </HintPath>
          </Reference>
        </xsl:if>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-name" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-name]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-name]) > 0">

              <xsl:variable name="extern"
                select="/Input/Projects/ExternalProject[@Name=$include-name]" />

              <xsl:for-each select="$extern/Binary">
                <Reference>
                  <xsl:attribute name="Include">
                    <xsl:value-of select="@Name" />
                  </xsl:attribute>
                  <xsl:if test="@Aliases != ''">
                    <Aliases><xsl:value-of select="@Aliases" /></Aliases>
                  </xsl:if>
                  <HintPath>
                    <xsl:value-of
                      select="user:GetRelativePath(
                        concat(
                          $project/@Path,
                          '\',
                          $project/@Name,
                          '.',
                          /Input/Generation/Platform,
                          '.csproj'),
                        @Path)" />
                  </HintPath>
                </Reference>
              </xsl:for-each>
              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./Binary">
                  <Reference>
                    <xsl:attribute name="Include">
                      <xsl:value-of select="@Name" />
                    </xsl:attribute>
                    <xsl:if test="@Aliases != ''">
                      <Aliases><xsl:value-of select="@Aliases" /></Aliases>
                    </xsl:if>
                    <HintPath>
                      <xsl:value-of
                        select="user:GetRelativePath(
                          concat(
                            $project/@Path,
                            '\',
                            $project/@Name,
                            '.',
                            /Input/Generation/Platform,
                            '.csproj'),
                          @Path)" />
                    </HintPath>
                  </Reference>
                </xsl:for-each>
              </xsl:for-each>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>

        <xsl:for-each select="/Input/NuGet/Package">
          <Reference>
            <xsl:attribute name="Include">
              <xsl:value-of select="@Name" />
            </xsl:attribute>
            <HintPath>
              <xsl:value-of
                select="user:GetRelativePath(
                  concat(
                    $project/@Path,
                    '\',
                    $project/@Name,
                    '.',
                    /Input/Generation/Platform,
                    '.csproj'),
                  .)" />
            </HintPath>
          </Reference>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/Compile">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/None">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/Content">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/EmbeddedResource">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/EmbeddedShaderProgram">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <xsl:if test="/Input/Generation/Platform = 'Web'">
        <xsl:if test="$project/@Type = 'App' or $project/@Type = 'Console' or $project/@Type = 'GUI' or $project/@Type = 'GTK'">
          <ItemGroup>
            <xsl:for-each select="/Input/Generation/JSILLibraries/Library">
              <None>
                <xsl:attribute name="Include">
                  <xsl:value-of select="./@Path" />
                </xsl:attribute>
                <Link>
                  <xsl:text>Libraries\</xsl:text>
                  <xsl:value-of select="./@Name" />
                </Link>
                <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
              </None>
            </xsl:for-each>
            <None Include="index.htm">
              <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
            </None>
          </ItemGroup>
        </xsl:if>
      </xsl:if>

      <ItemGroup>
        <xsl:for-each select="$project/Files/ShaderProgram">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/ApplicationDefinition">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/Page">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/AppxManifest">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/BundleResource">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/InterfaceDefinition">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/AndroidResource">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/SplashScreen">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/Files/Resource">
          <xsl:if test="user:ProjectIsActive(
              ./Platforms,
              ./IncludePlatforms,
              ./ExcludePlatforms,
              /Input/Generation/Platform)">
            <xsl:element
              name="{name()}"
              namespace="http://schemas.microsoft.com/developer/msbuild/2003">
              <xsl:attribute name="Include">
                <xsl:value-of select="@Include" />
              </xsl:attribute>
              <xsl:apply-templates select="node()"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <ItemGroup>
        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-path" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/ContentProject[@Name=$include-path]) > 0">

            <xsl:for-each select="/Input
                                  /Projects
                                  /ContentProject[@Name=$include-path]
                                  /Compiled">
              <xsl:choose>
                <xsl:when test="/Input/Generation/Platform = 'Windows8'">
                  <Content>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                      concat(
                        /Input/Generation/RootPath,
                        $project/@Path,
                        '\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj'),
                      current()/FullPath)" />
                    </xsl:attribute>
                    <Link>
                      <xsl:text>Content</xsl:text>
                      <xsl:value-of select="current()/RelativePath" />
                    </Link>
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                  </Content>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
                  <AndroidAsset>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                      concat(
                        /Input/Generation/RootPath,
                        $project/@Path,
                        '\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj'),
                      current()/FullPath)" />
                    </xsl:attribute>
                    <Link>
                      <xsl:text>Assets</xsl:text>
                      <xsl:value-of select="current()/RelativePath" />
                    </Link>
                  </AndroidAsset>
                </xsl:when>
                <xsl:when test="/Input/Generation/Platform = 'MacOS' or /Input/Generation/Platform = 'iOS'">
                  <Content>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                      concat(
                        /Input/Generation/RootPath,
                        $project/@Path,
                        '\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj'),
                      current()/FullPath)" />
                    </xsl:attribute>
                    <Link>
                      <xsl:text>Content</xsl:text>
                      <xsl:value-of select="current()/RelativePath" />
                    </Link>
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                  </Content>
                </xsl:when>
                <xsl:otherwise>
                  <None>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                      concat(
                        /Input/Generation/RootPath,
                        $project/@Path,
                        '\',
                        $project/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj'),
                      current()/FullPath)" />
                    </xsl:attribute>
                    <Link>
                      <xsl:text>Content</xsl:text>
                      <xsl:value-of select="current()/RelativePath" />
                    </Link>
                    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                  </None>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:for-each>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <xsl:choose>
        <xsl:when test="/Input/Generation/Platform = 'Android' or /Input/Generation/Platform = 'Ouya'">
          <Import Project="$(MSBuildExtensionsPath)\Novell\Novell.MonoDroid.CSharp.targets" />
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'Windows8'">
          <PropertyGroup Condition=" '$(VisualStudioVersion)' == '' or '$(VisualStudioVersion)' &lt; '11.0' ">
            <VisualStudioVersion>11.0</VisualStudioVersion>
          </PropertyGroup>
          <Import Project="$(MSBuildExtensionsPath)\Microsoft\WindowsXaml\v$(VisualStudioVersion)\Microsoft.Windows.UI.Xaml.CSharp.targets" />
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7' or /Input/Generation/Platform = 'Windows' or /Input/Generation/Platform = 'XBox360'">
          <Import Project="$(MSBuildExtensionsPath)\Microsoft\XNA Game Studio\Microsoft.Xna.GameStudio.targets" />
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
          <Import Project="$(MSBuildExtensionsPath)\Microsoft\$(TargetFrameworkIdentifier)\$(TargetFrameworkVersion)\Microsoft.$(TargetFrameworkIdentifier).$(TargetFrameworkVersion).Overrides.targets" />
          <Import Project="$(MSBuildExtensionsPath)\Microsoft\$(TargetFrameworkIdentifier)\$(TargetFrameworkVersion)\Microsoft.$(TargetFrameworkIdentifier).CSharp.targets" />
          <xsl:if test="user:IsTrue(/Input/Properties/RemoveXnaAssembliesOnWP8)">
            <Target Name="MonoGame_RemoveXnaAssemblies" AfterTargets="ImplicitlyExpandTargetFramework">
              <Message Text="MonoGame - Removing XNA Assembly references!" Importance="normal" />
              <ItemGroup>
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.dll'" />
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.GamerServices.dll'" />
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.GamerServicesExtensions.dll'" />
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.Input.Touch.dll'" />
                <ReferencePath Remove="@(ReferencePath)" Condition="'%(Filename)%(Extension)'=='Microsoft.Xna.Framework.MediaLibraryExtensions.dll'" />
              </ItemGroup>
            </Target>
          </xsl:if>
        </xsl:when>
        <xsl:when test="/Input/Generation/Platform = 'PSM'">
          <Import Project="$(MSBuildExtensionsPath)\Sce\Sce.Psm.CSharp.targets" />
        </xsl:when>
        <xsl:otherwise>
          <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
        </xsl:otherwise>
      </xsl:choose>

      <xsl:if test="/Input/Generation/Platform = 'Web'">
        <xsl:if test="$project/@Type = 'App' or $project/@Type = 'Console' or $project/@Type = 'GUI' or $project/@Type = 'GTK'">

          <xsl:choose>
            <xsl:when test="user:IsTrue(/Input/Generation/Properties/IgnoreWebPlatform)">
            </xsl:when>
            <xsl:otherwise>
              <Target Name="JSILCompile" AfterTargets="AfterBuild">
                <Exec>
                  <xsl:attribute name="WorkingDirectory">
                    <xsl:value-of
                      select="/Input/Generation/JSILDirectory" />
                  </xsl:attribute>
                  <xsl:attribute name="Command">
                    <xsl:if test="/Input/Generation/HostPlatform = 'Linux' or /Input/Generation/HostPlatform = 'MacOS'">
                      <xsl:text>mono </xsl:text>
                    </xsl:if>
                    <xsl:value-of select="/Input/Generation/JSILCompilerFile" />
                    <xsl:text> "</xsl:text>
                    <xsl:value-of select="/Input/Generation/RootPath" />
                    <xsl:value-of select="$project/@Path" />
                    <xsl:if test="/Input/Generation/HostPlatform = 'Linux' or /Input/Generation/HostPlatform = 'MacOS'">
                      <xsl:text>/bin/</xsl:text>
                      <xsl:if test="user:IsTrue(/Input/Properties/PlatformSpecificOutputFolder)">
                        <xsl:value-of select="/Input/Generation/Platform" />
                        <xsl:text>/$(Platform)/</xsl:text>
                      </xsl:if>
                      <xsl:text>$(Configuration)/</xsl:text>
                    </xsl:if>
                    <xsl:if test="/Input/Generation/HostPlatform = 'Windows'">
                      <xsl:text>\bin\</xsl:text>
                      <xsl:if test="user:IsTrue(/Input/Properties/PlatformSpecificOutputFolder)">
                        <xsl:value-of select="/Input/Generation/Platform" />
                        <xsl:text>\$(Platform)\</xsl:text>
                      </xsl:if>
                      <xsl:text>$(Configuration)\</xsl:text>
                    </xsl:if>
                    <xsl:choose>
                      <xsl:when test="/Input/Properties/AssemblyName">
                        <xsl:value-of select="/Input/Properties/AssemblyName" />
                        <xsl:text>.exe</xsl:text>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$project/@Name" />
                        <xsl:text>.exe</xsl:text>
                      </xsl:otherwise>
                    </xsl:choose>
                    <xsl:text>" --out="</xsl:text>
                    <xsl:value-of select="/Input/Generation/RootPath" />
                    <xsl:value-of select="$project/@Path" />
                    <xsl:if test="/Input/Generation/HostPlatform = 'Linux' or /Input/Generation/HostPlatform = 'MacOS'">
                      <xsl:text>/bin/</xsl:text>
                      <xsl:if test="user:IsTrue(/Input/Properties/PlatformSpecificOutputFolder)">
                        <xsl:value-of select="/Input/Generation/Platform" />
                        <xsl:text>/$(Platform)/</xsl:text>
                      </xsl:if>
                      <xsl:text>$(Configuration)</xsl:text>
                    </xsl:if>
                    <xsl:if test="/Input/Generation/HostPlatform = 'Windows'">
                      <xsl:text>\bin\</xsl:text>
                      <xsl:if test="user:IsTrue(/Input/Properties/PlatformSpecificOutputFolder)">
                        <xsl:value-of select="/Input/Generation/Platform" />
                        <xsl:text>\$(Platform)\</xsl:text>
                      </xsl:if>
                      <xsl:text>$(Configuration)</xsl:text>
                    </xsl:if>
                    <xsl:text>"</xsl:text>
                  </xsl:attribute>
                </Exec>
              </Target>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
      </xsl:if>

      {ADDITIONAL_TRANSFORMS}

      <ItemGroup>
        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-name" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-name]) = 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-name]) > 0">

              <xsl:variable name="extern"
                select="/Input/Projects/ExternalProject[@Name=$include-name]" />

              <xsl:for-each select="$extern/Project">
                <ProjectReference>
                  <xsl:attribute name="Include">
                    <xsl:value-of
                      select="user:GetRelativePath(
                        concat(
                          $project/@Path,
                          '\',
                          $project/@Name,
                          '.',
                          /Input/Generation/Platform,
                          '.csproj'),
                        ./@Path)" />
                  </xsl:attribute>
                  <Project>{<xsl:value-of select="./@Guid" />}</Project>
                  <Name><xsl:value-of select="./@Name" /></Name>
                </ProjectReference>
              </xsl:for-each>

              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./Project">
                  <ProjectReference>
                    <xsl:attribute name="Include">
                      <xsl:value-of
                        select="user:GetRelativePath(
                          concat(
                            $project/@Path,
                            '\',
                            $project/@Name,
                            '.',
                            /Input/Generation/Platform,
                            '.csproj'),
                          ./@Path)" />
                    </xsl:attribute>
                    <Project>{<xsl:value-of select="./@Guid" />}</Project>
                    <Name><xsl:value-of select="./@Name" /></Name>
                  </ProjectReference>
                </xsl:for-each>
              </xsl:for-each>

              <xsl:for-each select="$extern/Reference">
                <xsl:variable name="refd-name" select="./@Include" />
                <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) > 0">
                  <xsl:variable name="refd"
                    select="/Input/Projects/Project[@Name=$refd-name]" />

                  <xsl:if test="user:ProjectIsActive(
                    $refd/@Platforms,
                    '',
                    '',
                    /Input/Generation/Platform)">

                    <ProjectReference>
                      <xsl:attribute name="Include">
                        <xsl:value-of
                          select="user:GetRelativePath(
                            concat(
                              $project/@Path,
                              '\',
                              $project/@Name,
                              '.',
                              /Input/Generation/Platform,
                              '.csproj'),
                            concat(
                              $refd/@Path,
                              '\',
                              $refd/@Name,
                              '.',
                              /Input/Generation/Platform,
                              '.csproj'))" />
                      </xsl:attribute>
                      <Project>{<xsl:value-of select="$refd/@Guid" />}</Project>
                      <Name><xsl:value-of select="$refd/@Name" /><xsl:text>.</xsl:text><xsl:value-of select="/Input/Generation/Platform" /></Name>
                    </ProjectReference>
                  </xsl:if>

                </xsl:if>
              </xsl:for-each>


              <xsl:for-each select="$extern/Platform
                                      [@Type=/Input/Generation/Platform]">
                <xsl:for-each select="./Reference">
                  <xsl:variable name="refd-name" select="./@Include" />
                  <xsl:if test="count(/Input/Projects/Project[@Name=$refd-name]) > 0">
                    <xsl:variable name="refd"
                      select="/Input/Projects/Project[@Name=$refd-name]" />

                    <xsl:if test="user:ProjectIsActive(
                      $refd/@Platforms,
                      '',
                      '',
                      /Input/Generation/Platform)">

                      <ProjectReference>
                        <xsl:attribute name="Include">
                          <xsl:value-of
                            select="user:GetRelativePath(
                              concat(
                                $project/@Path,
                                '\',
                                $project/@Name,
                                '.',
                                /Input/Generation/Platform,
                                '.csproj'),
                              concat(
                                $refd/@Path,
                                '\',
                                $refd/@Name,
                                '.',
                                /Input/Generation/Platform,
                                '.csproj'))" />
                        </xsl:attribute>
                        <Project>{<xsl:value-of select="$refd/@Guid" />}</Project>
                        <Name><xsl:value-of select="$refd/@Name" /><xsl:text>.</xsl:text><xsl:value-of select="/Input/Generation/Platform" /></Name>
                      </ProjectReference>
                    </xsl:if>

                  </xsl:if>
                </xsl:for-each>
              </xsl:for-each>

            </xsl:if>
          </xsl:if>
        </xsl:for-each>

        <xsl:for-each select="$project/References/Reference">
          <xsl:variable name="include-path" select="./@Include" />
          <xsl:if test="
            count(/Input/Projects/Project[@Name=$include-path]) > 0">
            <xsl:if test="
              count(/Input/Projects/ExternalProject[@Name=$include-path]) = 0">

              <xsl:if test="user:ProjectIsActive(
                $project/@Platforms,
                '',
                '',
                /Input/Generation/Platform)">

                <ProjectReference>
                  <xsl:attribute name="Include">
                    <xsl:value-of
                      select="user:GetRelativePath(
                        concat(
                          $project/@Path,
                          '\',
                          $project/@Name,
                          '.',
                          /Input/Generation/Platform,
                          '.csproj'),
                        concat(
                          /Input/Projects/Project[@Name=$include-path]/@Path,
                          '\',
                          @Include,
                          '.',
                          /Input/Generation/Platform,
                          '.csproj'))" />
                  </xsl:attribute>
                  <Project>{<xsl:value-of select="/Input/Projects/Project[@Name=$include-path]/@Guid" />}</Project>
                  <Name><xsl:value-of select="@Include" /><xsl:text>.</xsl:text><xsl:value-of select="/Input/Generation/Platform" /></Name>
                </ProjectReference>
              </xsl:if>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </ItemGroup>

      <xsl:if test="/Input/Properties/MonoDevelopPoliciesFile">
        <ProjectExtensions>
          <MonoDevelop>
            <Properties>
              <xsl:value-of
                select="user:ReadFile(concat(/Input/Generation/RootPath, '\', /Input/Properties/MonoDevelopPoliciesFile))"
                disable-output-escaping="yes" />
            </Properties>
          </MonoDevelop>
        </ProjectExtensions>
      </xsl:if>

    </Project>

  </xsl:template>

  <xsl:template match="*">
    <xsl:element
      name="{name()}"
      namespace="http://schemas.microsoft.com/developer/msbuild/2003">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>
