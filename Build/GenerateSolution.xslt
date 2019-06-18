<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:user="urn:my-scripts"
  exclude-result-prefixes="xsl msxsl user"
  version="1.0">
  
  <xsl:output method="text" indent="no" />
 
  <msxsl:script language="C#" implements-prefix="user">
    <msxsl:assembly name="System.Web" />
    <msxsl:using namespace="System" />
    <msxsl:using namespace="System.Web" />
    <![CDATA[
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
    
    public bool ProjectIsActive(string platformString, string activePlatform)
    {
      if (string.IsNullOrEmpty(platformString))
      {
        return true;
      }
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
    ]]>
  </msxsl:script> 
 
  <xsl:template match="/">
    <xsl:choose>
      <xsl:when test="/Input/Generation/Platform = 'Windows8' or /Input/Generation/Platform = 'WindowsPhone' or /Input/Generation/Platform = 'WindowsGL' or /Input/Generation/Platform = 'WindowsDX' or /Input/Generation/Platform = 'Android'">
<xsl:text>Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2012
</xsl:text>
      </xsl:when>
      <xsl:otherwise>
<xsl:text>Microsoft Visual Studio Solution File, Format Version 11.00
# Visual Studio 2010
</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:for-each select="/Input/Projects/Project">
      <xsl:if test="user:ProjectIsActive(
          current()/@Platforms,
          /Input/Generation/Platform)">
        <xsl:call-template name="project-definition">
          <xsl:with-param name="type" select="current()/@Type" />
          <!--<xsl:with-param name="name" select="current()/@Name" />-->
          <xsl:with-param name="name" select="concat(
                        current()/@Name,
                        '.',
                        /Input/Generation/Platform)" />
          <xsl:with-param name="guid" select="current()/@Guid" />
          <xsl:with-param name="path" select="concat(
                        current()/@Path,
                        '\',
                        current()/@Name,
                        '.',
                        /Input/Generation/Platform,
                        '.csproj')" />
        </xsl:call-template>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="/Input/Projects/ExternalProject/Project">
      <xsl:call-template name="project-definition">
        <xsl:with-param name="type" select="current()/@Type" />
        <xsl:with-param name="name" select="current()/@Name" />
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="path" select="current()/@Path" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:for-each select="/Input/Projects/ExternalProject
                          /Platform[@Type=/Input/Generation/Platform]
                          /Project">
      <xsl:call-template name="project-definition">
        <xsl:with-param name="type" select="current()/@Type" />
        <xsl:with-param name="name" select="current()/@Name" />
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="path" select="current()/@Path" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:choose>
      <xsl:when test="/Input/Generation/Platform = 'iOS'">
        <xsl:text>Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|iPhoneSimulator = Debug|iPhoneSimulator
                Release|iPhoneSimulator = Release|iPhoneSimulator
                Debug|iPhone = Debug|iPhone
                Release|iPhone = Release|iPhone
                Ad-Hoc|iPhone = Ad-Hoc|iPhone
                AppStore|iPhone = AppStore|iPhone
        EndGlobalSection
        GlobalSection(ProjectConfigurationPlatforms) = postSolution
</xsl:text>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
        <xsl:text>Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|x86 = Debug|x86
                Release|x86 = Release|x86
                Debug|ARM = Debug|ARM
                Release|ARM = Release|ARM
        EndGlobalSection
        GlobalSection(ProjectConfigurationPlatforms) = postSolution
</xsl:text>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'Windows'">
        <xsl:text>Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|x86 = Debug|x86
                Release|x86 = Release|x86
        EndGlobalSection
        GlobalSection(ProjectConfigurationPlatforms) = postSolution
</xsl:text>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7'">
        <xsl:text>Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|Any CPU = Debug|Windows Phone
                Release|Any CPU = Release|Windows Phone
                Debug|Windows Phone = Debug|Windows Phone
                Release|Windows Phone = Release|Windows Phone
        EndGlobalSection
        GlobalSection(ProjectConfigurationPlatforms) = postSolution
</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Global
        GlobalSection(SolutionConfigurationPlatforms) = preSolution
                Debug|Any CPU = Debug|Any CPU
                Release|Any CPU = Release|Any CPU
        EndGlobalSection
        GlobalSection(ProjectConfigurationPlatforms) = postSolution
</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    
    <xsl:for-each select="/Input/Projects/Project">
      <xsl:if test="user:ProjectIsActive(
          current()/@Platforms,
          /Input/Generation/Platform)">
        <xsl:call-template name="project-configuration">
          <xsl:with-param name="guid" select="current()/@Guid" />
          <xsl:with-param name="root" select="current()" />
        </xsl:call-template>
      </xsl:if>
    </xsl:for-each>
    <xsl:for-each select="/Input/Projects/ExternalProject/Project">
      <xsl:call-template name="project-configuration">
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="root" select="current()" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:for-each select="/Input/Projects/ExternalProject
                          /Platform[@Type=/Input/Generation/Platform]
                          /Project">
      <xsl:call-template name="project-configuration">
        <xsl:with-param name="guid" select="current()/@Guid" />
        <xsl:with-param name="root" select="current()" />
      </xsl:call-template>
    </xsl:for-each>
    <xsl:text>        EndGlobalSection
EndGlobal
</xsl:text>
  </xsl:template>
  
  <xsl:template name="project-definition">
    <xsl:param name="name" />
    <xsl:param name="type" />
    <xsl:param name="path" />
    <xsl:param name="guid" />
    <xsl:text>Project("{</xsl:text>
    <xsl:choose>
      <xsl:when test="$type = 'Content'">
        <xsl:text>9344BDBB-3E7F-41FC-A0DD-8665D75EE146</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>FAE04EC0-301F-11D3-BF4B-00C04F79EFBC</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>}") = </xsl:text>
    <xsl:text>"</xsl:text>
    <xsl:value-of select="$name" />
    <xsl:text>", "</xsl:text>
    <xsl:value-of select="$path" />
    <xsl:text>", "{</xsl:text>
    <xsl:value-of select="$guid" />
    <xsl:text>}"
EndProject
</xsl:text>
  </xsl:template>
  
  <xsl:template name="project-configuration">
    <xsl:param name="guid" />
    <xsl:param name="root" />
    <xsl:variable name="adhoc-mapping">
      <xsl:value-of select="$root/ConfigurationMapping[@Old='Ad-Hoc']/@New" />
    </xsl:variable>
    <xsl:variable name="appstore-mapping">
      <xsl:value-of select="$root/ConfigurationMapping[@Old='AppStore']/@New" />
    </xsl:variable>
    <xsl:variable name="debug-mapping">
      <xsl:value-of select="$root/ConfigurationMapping[@Old='Debug']/@New" />
    </xsl:variable>
    <xsl:variable name="release-mapping">
      <xsl:value-of select="$root/ConfigurationMapping[@Old='Release']/@New" />
    </xsl:variable>
    
    <xsl:choose>
      <xsl:when test="/Input/Generation/Platform = 'iOS'">
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Ad-Hoc|iPhone.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$adhoc-mapping != ''">
            <xsl:value-of select="$adhoc-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Ad-Hoc|iPhone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Ad-Hoc|iPhone.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$adhoc-mapping != ''">
            <xsl:value-of select="$adhoc-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Ad-Hoc|iPhone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.AppStore|iPhone.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$appstore-mapping != ''">
            <xsl:value-of select="$appstore-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>AppStore|iPhone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.AppStore|iPhone.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$appstore-mapping != ''">
            <xsl:value-of select="$appstore-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>AppStore|iPhone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|iPhone.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|iPhone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|iPhone.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|iPhone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|iPhoneSimulator.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|iPhoneSimulator</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|iPhoneSimulator.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|iPhoneSimulator</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|iPhone.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|iPhone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|iPhone.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|iPhone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|iPhoneSimulator.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|iPhoneSimulator</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|iPhoneSimulator.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|iPhoneSimulator</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'WindowsPhone'">
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|x86.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|x86.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|x86.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|x86.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|ARM.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|ARM</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|ARM.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|ARM</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|ARM.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|ARM</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|ARM.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|ARM</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'WindowsPhone7'">
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|Windows Phone.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|Windows Phone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|Windows Phone.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|Windows Phone.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|Windows Phone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|Windows Phone.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|Windows Phone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|Any CPU.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|Windows Phone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|Any CPU.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|Windows Phone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|Any CPU.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|Windows Phone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|Any CPU.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|Windows Phone</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
      </xsl:when>
      <xsl:when test="/Input/Generation/Platform = 'Windows'">
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|x86.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|x86.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|x86.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|x86.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|x86</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|Any CPU.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|Any CPU</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Debug|Any CPU.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$debug-mapping != ''">
            <xsl:value-of select="$debug-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Debug|Any CPU</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|Any CPU.ActiveCfg = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|Any CPU</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
        <xsl:text>                {</xsl:text>
        <xsl:value-of select="$guid" />
        <xsl:text>}.Release|Any CPU.Build.0 = </xsl:text>
        <xsl:choose>
          <xsl:when test="$release-mapping != ''">
            <xsl:value-of select="$release-mapping" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>Release|Any CPU</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>
</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="*">
    <xsl:element 
      name="{name()}" 
      namespace="http://schemas.microsoft.com/developer/msbuild/2003">
      <xsl:apply-templates select="@*|node()"/>
    </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>
