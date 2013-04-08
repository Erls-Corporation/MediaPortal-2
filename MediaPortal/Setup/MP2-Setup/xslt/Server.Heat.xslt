﻿<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
                xmlns="http://schemas.microsoft.com/wix/2006/wi"
                xmlns:wix="http://schemas.microsoft.com/wix/2006/wi"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:key name="MP2-Server.exe"
           match="//wix:Component[wix:File/@Source = '$(var.MediaPortal.Server.TargetDir)\MP2-Server.exe']"
           use="@Id"/>

  <!-- Copy all nodes from source to target and apply templates. -->
  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>

  <!-- Add comment to the beginning of the file and continue applying. -->
  <xsl:template match="wix:Wix">
    <Wix>
      <xsl:comment>This file was generated by the XSLT stylesheet 'Server.Heat.xslt'</xsl:comment>
      <xsl:apply-templates/>
    </Wix>
  </xsl:template>

  <!--
  Remove application executable from harvested file. It is explicitly defined
  in 'Features\*.wxs to reuse it for shortcut creation or service registration.
  -->
  <xsl:template match="wix:Component[@Id = key('MP2-Server.exe', @Id)/@Id]"/>
  <xsl:template match="wix:ComponentRef[@Id = key('MP2-Server.exe', @Id)/@Id]"/>

  <!-- Explicitly set DirectoryID to allow referencing to it. -->
  <xsl:template match="wix:DirectoryRef[@Id = 'INSTALLDIR_SERVER']/wix:Directory[@Name = 'Plugins']">
    <Directory Id="SERVER_PLUGINS_FOLDER" Name="Plugins">
      <xsl:apply-templates/>
    </Directory>
  </xsl:template>
</xsl:stylesheet>