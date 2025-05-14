#!/bin/bash
PROTECTION_STUDIO="/opt/guardant/protection_studio"
$PROTECTION_STUDIO --project ./guardant_config.pprx --output $1/Protected
chmod +x $1/Protected/* # Даем права на выполнение