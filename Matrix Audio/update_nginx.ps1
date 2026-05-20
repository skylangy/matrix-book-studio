
$piIP = "192.168.7.205"
$destinationPath = "/home/pi/scripts"
$file = "nginx.conf"
$remotePath = "andy@$piIP`:$destinationPath"

Write-Host "[INFO] Copying $file to $remotePath"
& scp -p $file $remotePath

if ($LASTEXITCODE -eq 0) {
    Write-Host "[INFO] Successfully transferred $file to $ip" -ForegroundColor Green
}
else {
    Write-Host "[INFO] Failed to transfer $file to $ip" -ForegroundColor Red
}

Write-Host "[INFO] Copy nginx.conf to etc/..." -ForegroundColor Green
& ssh andy@$piIP "sudo mv $destinationPath/nginx.conf /etc/nginx/nginx.conf"
Write-Host "[INFO] nginx.conf is updated." -ForegroundColor Green

& ssh andy@$piIP "sudo nginx -t"
& ssh andy@$piIP "sudo systemctl restart nginx"
