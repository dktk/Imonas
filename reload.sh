
echo ">>> Reloading $1"

echo ">>> Reloading daemon"
sudo systemctl daemon-reload

echo ">>> Restart service"
sudo systemctl restart $1

echo ">>> Journal"
sudo journalctl -u $1 -n 80 --no-pager

echo ">>> Reloading DONE"
