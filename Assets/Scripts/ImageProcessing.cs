using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImageProcessing
{
    public class ImageProcessing
    {
        public static int[,] Erode(int[,] image, int radius)
        {
            int[,] tempImage = (int[,])image.Clone();
            int width = image.GetLength(0);
            int height = image.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (image[x, y] == 1)
                    {
                        bool shouldErode = false;

                        for (int i = x - radius; i <= x + radius; i++)
                        {
                            if (shouldErode) break;
                            for (int j = y - radius; j <= y + radius; j++)
                            {
                                if (shouldErode) break;
                                if ((i - x) * (i - x) + (j - y) * (j - y) < radius * radius)
                                {
                                    if (i >= 0 && i < width && j >= 0 && j < height)
                                    {
                                        //Debug.Log(i + " " + j);
                                        if (image[i ,j] != 1)
                                        {
                                            shouldErode = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (shouldErode)
                        {
                            tempImage[x, y] = 0;
                        }
                    }
                }
            }

            return tempImage;
        }

        public static int[,] Dilate(int[,] image, int radius)
        {
            int[,] tempImage = (int[,])image.Clone();
            int width = image.GetLength(0);
            int height = image.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (image[x, y] != 1)
                    {
                        bool shouldDilate = false;

                        for (int i = x - radius; i <= x + radius; i++)
                        {
                            if (shouldDilate) break;
                            for (int j = y - radius; j <= y + radius; j++)
                            {
                                if (shouldDilate) break;
                                if ((i - x) * (i - x) + (j - y) * (j - y) <= radius * radius)
                                {
                                    if (i >= 0 && y < width && j >= 0 && j < height)
                                    {
                                        if (image[i, j] == 1)
                                        {
                                            shouldDilate = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (shouldDilate)
                        {
                            tempImage[x, y] = 1;
                        }
                    }
                }
            }

            return tempImage;
        }
    }
}
